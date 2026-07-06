using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Data;
using StoreManagementSystem.DTOs.Dashboard;
using StoreManagementSystem.DTOs.Inventory;
using StoreManagementSystem.Interfaces;

namespace StoreManagementSystem.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalSales = await _context.Sales.SumAsync(s => (decimal?)s.Total) ?? 0;
        var totalExpenses = await _context.Expenses.SumAsync(e => (decimal?)e.Amount) ?? 0;

        var todaysSales = await _context.Sales
            .Where(s => s.CreatedAt >= todayStart)
            .SumAsync(s => (decimal?)s.Total) ?? 0;

        var monthlySales = await _context.Sales
            .Where(s => s.CreatedAt >= monthStart)
            .SumAsync(s => (decimal?)s.Total) ?? 0;

        var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
        var totalEmployees = await _context.Employees.CountAsync(e => e.IsActive);

        var lowStockProducts = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.Quantity <= p.LowStockThreshold)
            .OrderBy(p => p.Quantity)
            .Select(p => new LowStockProductDto
            {
                ProductId = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                Quantity = p.Quantity,
                LowStockThreshold = p.LowStockThreshold,
                CategoryName = p.Category!.Name
            })
            .ToListAsync();

        return new DashboardSummaryDto
        {
            TotalSales = totalSales,
            TotalExpenses = totalExpenses,
            TotalProfit = totalSales - totalExpenses,
            TotalProducts = totalProducts,
            TotalEmployees = totalEmployees,
            TodaysSales = todaysSales,
            MonthlySales = monthlySales,
            LowStockProducts = lowStockProducts
        };
    }
}
