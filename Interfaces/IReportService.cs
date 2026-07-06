using StoreManagementSystem.DTOs.Reports;

namespace StoreManagementSystem.Interfaces;

public interface IReportService
{
    Task<SalesReportDto> GetSalesReportAsync(DateTime from, DateTime to);
    Task<ExpenseReportDto> GetExpenseReportAsync(DateTime from, DateTime to);
    Task<InventoryReportDto> GetInventoryReportAsync(DateTime from, DateTime to);
    Task<PayrollReportDto> GetPayrollReportAsync(DateTime from, DateTime to);
    Task<ProfitAndLossReportDto> GetProfitAndLossReportAsync(DateTime from, DateTime to);
}
