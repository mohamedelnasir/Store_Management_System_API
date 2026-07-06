using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Data;
using StoreManagementSystem.DTOs.Payroll;
using StoreManagementSystem.Interfaces;
using StoreManagementSystem.Middleware;
using StoreManagementSystem.Models;

namespace StoreManagementSystem.Services;

public class PayrollService : IPayrollService
{
    private readonly ApplicationDbContext _context;

    public PayrollService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PayrollDto>> GetAllAsync(int? employeeId, int? month, int? year)
    {
        var query = _context.Payrolls.Include(p => p.Employee).AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(p => p.EmployeeId == employeeId.Value);
        }

        if (month.HasValue)
        {
            query = query.Where(p => p.Month == month.Value);
        }

        if (year.HasValue)
        {
            query = query.Where(p => p.Year == year.Value);
        }

        return await query
            .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
            .Select(p => ToDto(p))
            .ToListAsync();
    }

    public async Task<PayrollDto?> GetByIdAsync(int id)
    {
        var payroll = await _context.Payrolls.Include(p => p.Employee).FirstOrDefaultAsync(p => p.Id == id);
        return payroll is null ? null : ToDto(payroll);
    }

    public async Task<PayrollDto> GenerateAsync(GeneratePayrollDto dto)
    {
        var employee = await _context.Employees.FindAsync(dto.EmployeeId);
        if (employee is null)
        {
            throw new NotFoundException($"Employee with id {dto.EmployeeId} was not found.");
        }

        var alreadyExists = await _context.Payrolls.AnyAsync(p =>
            p.EmployeeId == dto.EmployeeId && p.Month == dto.Month && p.Year == dto.Year);

        if (alreadyExists)
        {
            throw new ConflictException(
                $"Payroll for this employee has already been generated for {dto.Month}/{dto.Year}.");
        }

        // Net Salary = Salary + Bonus - Deduction
        var netSalary = employee.Salary + dto.Bonus - dto.Deduction;

        var payroll = new Payroll
        {
            EmployeeId = employee.Id,
            BaseSalary = employee.Salary,
            Bonus = dto.Bonus,
            Deduction = dto.Deduction,
            NetSalary = netSalary,
            Month = dto.Month,
            Year = dto.Year
        };

        _context.Payrolls.Add(payroll);
        await _context.SaveChangesAsync();

        await _context.Entry(payroll).Reference(p => p.Employee).LoadAsync();
        return ToDto(payroll);
    }

    private static PayrollDto ToDto(Payroll p) => new()
    {
        Id = p.Id,
        EmployeeId = p.EmployeeId,
        EmployeeName = p.Employee?.Name ?? string.Empty,
        BaseSalary = p.BaseSalary,
        Bonus = p.Bonus,
        Deduction = p.Deduction,
        NetSalary = p.NetSalary,
        Month = p.Month,
        Year = p.Year,
        CreatedAt = p.CreatedAt
    };
}
