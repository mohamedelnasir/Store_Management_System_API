using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Data;
using StoreManagementSystem.DTOs.Employees;
using StoreManagementSystem.Interfaces;
using StoreManagementSystem.Models;

namespace StoreManagementSystem.Services;

public class EmployeeService : IEmployeeService
{
    private readonly ApplicationDbContext _context;

    public EmployeeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync(string? search)
    {
        var query = _context.Employees.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e => e.Name.Contains(search) || e.Position.Contains(search));
        }

        return await query
            .OrderBy(e => e.Name)
            .Select(e => ToDto(e))
            .ToListAsync();
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        return employee is null ? null : ToDto(employee);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto)
    {
        var employee = new Employee
        {
            Name = dto.Name,
            Phone = dto.Phone,
            Position = dto.Position,
            Salary = dto.Salary,
            HireDate = dto.HireDate,
            IsActive = true
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return ToDto(employee);
    }

    public async Task<EmployeeDto?> UpdateAsync(int id, UpdateEmployeeDto dto)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee is null)
        {
            return null;
        }

        employee.Name = dto.Name;
        employee.Phone = dto.Phone;
        employee.Position = dto.Position;
        employee.Salary = dto.Salary;
        employee.HireDate = dto.HireDate;
        employee.IsActive = dto.IsActive;
        employee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ToDto(employee);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var employee = await _context.Employees.Include(e => e.Payrolls).FirstOrDefaultAsync(e => e.Id == id);
        if (employee is null)
        {
            return false;
        }

        if (employee.Payrolls.Any())
        {
            throw new Middleware.ConflictException(
                "Cannot delete an employee with existing payroll history. Deactivate the employee instead.");
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        return true;
    }

    private static EmployeeDto ToDto(Employee e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Phone = e.Phone,
        Position = e.Position,
        Salary = e.Salary,
        HireDate = e.HireDate,
        IsActive = e.IsActive
    };
}
