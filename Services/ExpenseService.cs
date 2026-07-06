using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Data;
using StoreManagementSystem.DTOs.Expenses;
using StoreManagementSystem.Interfaces;
using StoreManagementSystem.Models;

namespace StoreManagementSystem.Services;

public class ExpenseService : IExpenseService
{
    private readonly ApplicationDbContext _context;

    public ExpenseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ExpenseDto>> GetAllAsync(DateTime? from, DateTime? to)
    {
        var query = _context.Expenses.Include(e => e.CreatedByUser).AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(e => e.ExpenseDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(e => e.ExpenseDate <= to.Value);
        }

        return await query
            .OrderByDescending(e => e.ExpenseDate)
            .Select(e => ToDto(e))
            .ToListAsync();
    }

    public async Task<ExpenseDto?> GetByIdAsync(int id)
    {
        var expense = await _context.Expenses.Include(e => e.CreatedByUser).FirstOrDefaultAsync(e => e.Id == id);
        return expense is null ? null : ToDto(expense);
    }

    public async Task<ExpenseDto> CreateAsync(CreateExpenseDto dto, int userId)
    {
        var expense = new Expense
        {
            Category = dto.Category,
            Amount = dto.Amount,
            Note = dto.Note,
            ExpenseDate = dto.ExpenseDate ?? DateTime.UtcNow,
            CreatedByUserId = userId
        };

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        await _context.Entry(expense).Reference(e => e.CreatedByUser).LoadAsync();
        return ToDto(expense);
    }

    public async Task<ExpenseDto?> UpdateAsync(int id, UpdateExpenseDto dto)
    {
        var expense = await _context.Expenses.Include(e => e.CreatedByUser).FirstOrDefaultAsync(e => e.Id == id);
        if (expense is null)
        {
            return null;
        }

        expense.Category = dto.Category;
        expense.Amount = dto.Amount;
        expense.Note = dto.Note;
        expense.ExpenseDate = dto.ExpenseDate;
        expense.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ToDto(expense);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense is null)
        {
            return false;
        }

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<MonthlyExpenseSummaryDto> GetMonthlySummaryAsync(int year, int month)
    {
        var expenses = await _context.Expenses
            .Where(e => e.ExpenseDate.Year == year && e.ExpenseDate.Month == month)
            .ToListAsync();

        var byCategory = expenses
            .GroupBy(e => e.Category)
            .Select(g => new CategoryExpenseTotalDto { Category = g.Key.ToString(), Total = g.Sum(e => e.Amount) })
            .OrderByDescending(c => c.Total)
            .ToList();

        return new MonthlyExpenseSummaryDto
        {
            Year = year,
            Month = month,
            Total = expenses.Sum(e => e.Amount),
            ByCategory = byCategory
        };
    }

    private static ExpenseDto ToDto(Expense e) => new()
    {
        Id = e.Id,
        Category = e.Category.ToString(),
        Amount = e.Amount,
        Note = e.Note,
        ExpenseDate = e.ExpenseDate,
        CreatedByUserName = e.CreatedByUser?.FullName ?? string.Empty,
        CreatedAt = e.CreatedAt
    };
}
