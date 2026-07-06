using StoreManagementSystem.DTOs.Expenses;

namespace StoreManagementSystem.Interfaces;

public interface IExpenseService
{
    Task<IEnumerable<ExpenseDto>> GetAllAsync(DateTime? from, DateTime? to);
    Task<ExpenseDto?> GetByIdAsync(int id);
    Task<ExpenseDto> CreateAsync(CreateExpenseDto dto, int userId);
    Task<ExpenseDto?> UpdateAsync(int id, UpdateExpenseDto dto);
    Task<bool> DeleteAsync(int id);
    Task<MonthlyExpenseSummaryDto> GetMonthlySummaryAsync(int year, int month);
}
