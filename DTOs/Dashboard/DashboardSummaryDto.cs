using StoreManagementSystem.DTOs.Inventory;

namespace StoreManagementSystem.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public decimal TotalSales { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalProfit { get; set; }
    public int TotalProducts { get; set; }
    public int TotalEmployees { get; set; }
    public decimal TodaysSales { get; set; }
    public decimal MonthlySales { get; set; }
    public List<LowStockProductDto> LowStockProducts { get; set; } = new();
}
