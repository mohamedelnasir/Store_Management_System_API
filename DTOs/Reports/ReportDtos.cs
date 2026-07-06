using StoreManagementSystem.DTOs.Expenses;

namespace StoreManagementSystem.DTOs.Reports;

public class DateRangeReportDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}

public class SalesReportDto : DateRangeReportDto
{
    public int TotalSalesCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<ProductSalesBreakdownDto> TopProducts { get; set; } = new();
}

public class ProductSalesBreakdownDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

public class ExpenseReportDto : DateRangeReportDto
{
    public decimal TotalExpenses { get; set; }
    public List<CategoryExpenseTotalDto> ByCategory { get; set; } = new();
}

public class InventoryReportDto : DateRangeReportDto
{
    public List<InventoryReportLineDto> Lines { get; set; } = new();
}

public class InventoryReportLineDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
    public int Purchased { get; set; }
    public int Sold { get; set; }
    public int Returned { get; set; }
    public int Damaged { get; set; }
    public int Adjusted { get; set; }
}

public class PayrollReportDto : DateRangeReportDto
{
    public decimal TotalNetSalaries { get; set; }
    public decimal TotalBonuses { get; set; }
    public decimal TotalDeductions { get; set; }
    public int EmployeeCount { get; set; }
}

public class ProfitAndLossReportDto : DateRangeReportDto
{
    public decimal Revenue { get; set; }
    public decimal CostOfGoodsSold { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit { get; set; }
}
