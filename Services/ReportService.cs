using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Data;
using StoreManagementSystem.DTOs.Expenses;
using StoreManagementSystem.DTOs.Reports;
using StoreManagementSystem.Interfaces;
using StoreManagementSystem.Models.Enums;

namespace StoreManagementSystem.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalesReportDto> GetSalesReportAsync(DateTime from, DateTime to)
    {
        var sales = await _context.Sales
            .Include(s => s.SaleItems).ThenInclude(si => si.Product)
            .Where(s => s.CreatedAt >= from && s.CreatedAt <= to)
            .ToListAsync();

        var topProducts = sales
            .SelectMany(s => s.SaleItems)
            .GroupBy(si => new { si.ProductId, Name = si.Product?.Name ?? string.Empty })
            .Select(g => new ProductSalesBreakdownDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                QuantitySold = g.Sum(si => si.Quantity),
                Revenue = g.Sum(si => si.LineTotal)
            })
            .OrderByDescending(p => p.Revenue)
            .ToList();

        return new SalesReportDto
        {
            From = from,
            To = to,
            TotalSalesCount = sales.Count,
            TotalRevenue = sales.Sum(s => s.Total),
            TopProducts = topProducts
        };
    }

    public async Task<ExpenseReportDto> GetExpenseReportAsync(DateTime from, DateTime to)
    {
        var expenses = await _context.Expenses
            .Where(e => e.ExpenseDate >= from && e.ExpenseDate <= to)
            .ToListAsync();

        var byCategory = expenses
            .GroupBy(e => e.Category)
            .Select(g => new CategoryExpenseTotalDto { Category = g.Key.ToString(), Total = g.Sum(e => e.Amount) })
            .OrderByDescending(c => c.Total)
            .ToList();

        return new ExpenseReportDto
        {
            From = from,
            To = to,
            TotalExpenses = expenses.Sum(e => e.Amount),
            ByCategory = byCategory
        };
    }

    public async Task<InventoryReportDto> GetInventoryReportAsync(DateTime from, DateTime to)
    {
        var transactions = await _context.InventoryTransactions
            .Include(t => t.Product)
            .Where(t => t.CreatedAt >= from && t.CreatedAt <= to)
            .ToListAsync();

        var lines = transactions
            .GroupBy(t => new { t.ProductId, Name = t.Product?.Name ?? string.Empty, CurrentQty = t.Product?.Quantity ?? 0 })
            .Select(g => new InventoryReportLineDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                CurrentQuantity = g.Key.CurrentQty,
                Purchased = g.Where(t => t.Type == InventoryTransactionType.Purchase).Sum(t => t.QuantityChange),
                Sold = -g.Where(t => t.Type == InventoryTransactionType.Sale).Sum(t => t.QuantityChange),
                Returned = g.Where(t => t.Type == InventoryTransactionType.Return).Sum(t => t.QuantityChange),
                Damaged = -g.Where(t => t.Type == InventoryTransactionType.Damaged).Sum(t => t.QuantityChange),
                Adjusted = g.Where(t => t.Type == InventoryTransactionType.Adjustment).Sum(t => t.QuantityChange)
            })
            .OrderBy(l => l.ProductName)
            .ToList();

        return new InventoryReportDto
        {
            From = from,
            To = to,
            Lines = lines
        };
    }

    public async Task<PayrollReportDto> GetPayrollReportAsync(DateTime from, DateTime to)
    {
        var payrolls = await _context.Payrolls
            .Where(p => new DateTime(p.Year, p.Month, 1) >= from.Date && new DateTime(p.Year, p.Month, 1) <= to.Date)
            .ToListAsync();

        return new PayrollReportDto
        {
            From = from,
            To = to,
            TotalNetSalaries = payrolls.Sum(p => p.NetSalary),
            TotalBonuses = payrolls.Sum(p => p.Bonus),
            TotalDeductions = payrolls.Sum(p => p.Deduction),
            EmployeeCount = payrolls.Select(p => p.EmployeeId).Distinct().Count()
        };
    }

    public async Task<ProfitAndLossReportDto> GetProfitAndLossReportAsync(DateTime from, DateTime to)
    {
        var saleItems = await _context.SaleItems
            .Include(si => si.Sale)
            .Include(si => si.Product)
            .Where(si => si.Sale != null && si.Sale.CreatedAt >= from && si.Sale.CreatedAt <= to)
            .ToListAsync();

        var revenue = saleItems.Sum(si => si.LineTotal);

        // Note: COGS uses each product's *current* BuyPrice as an approximation, since historical
        // buy price per sale isn't tracked. Fine for a class project; a production system would
        // snapshot the buy price onto the SaleItem at the time of sale for full accuracy.
        var costOfGoodsSold = saleItems.Sum(si => si.Quantity * (si.Product?.BuyPrice ?? 0));

        var totalExpenses = await _context.Expenses
            .Where(e => e.ExpenseDate >= from && e.ExpenseDate <= to)
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        var grossProfit = revenue - costOfGoodsSold;
        var netProfit = grossProfit - totalExpenses;

        return new ProfitAndLossReportDto
        {
            From = from,
            To = to,
            Revenue = revenue,
            CostOfGoodsSold = costOfGoodsSold,
            GrossProfit = grossProfit,
            TotalExpenses = totalExpenses,
            NetProfit = netProfit
        };
    }
}
