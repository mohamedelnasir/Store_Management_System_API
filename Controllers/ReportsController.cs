using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagementSystem.DTOs.Reports;
using StoreManagementSystem.Interfaces;
using StoreManagementSystem.Middleware;

namespace StoreManagementSystem.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Admin,Manager")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("sales")]
    public async Task<ActionResult<SalesReportDto>> GetSalesReport(
        [FromQuery] string period = "monthly", [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var (start, end) = ResolveRange(period, from, to);
        return Ok(await _reportService.GetSalesReportAsync(start, end));
    }

    [HttpGet("expenses")]
    public async Task<ActionResult<ExpenseReportDto>> GetExpenseReport(
        [FromQuery] string period = "monthly", [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var (start, end) = ResolveRange(period, from, to);
        return Ok(await _reportService.GetExpenseReportAsync(start, end));
    }

    [HttpGet("inventory")]
    public async Task<ActionResult<InventoryReportDto>> GetInventoryReport(
        [FromQuery] string period = "monthly", [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var (start, end) = ResolveRange(period, from, to);
        return Ok(await _reportService.GetInventoryReportAsync(start, end));
    }

    /// <summary>Payroll touches salary data, so this report is restricted to Admins even though the
    /// rest of the Reports module is open to Managers.</summary>
    [HttpGet("payroll")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PayrollReportDto>> GetPayrollReport(
        [FromQuery] string period = "monthly", [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var (start, end) = ResolveRange(period, from, to);
        return Ok(await _reportService.GetPayrollReportAsync(start, end));
    }

    [HttpGet("profit-and-loss")]
    public async Task<ActionResult<ProfitAndLossReportDto>> GetProfitAndLossReport(
        [FromQuery] string period = "monthly", [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var (start, end) = ResolveRange(period, from, to);
        return Ok(await _reportService.GetProfitAndLossReportAsync(start, end));
    }

    /// <summary>
    /// Resolves a (start, end) range from a named period ("daily", "weekly", "monthly") or,
    /// for period="custom", from the explicit from/to query params.
    /// </summary>
    private static (DateTime Start, DateTime End) ResolveRange(string period, DateTime? from, DateTime? to)
    {
        var now = DateTime.UtcNow;

        switch (period.Trim().ToLowerInvariant())
        {
            case "daily":
                return (now.Date, now.Date.AddDays(1).AddTicks(-1));

            case "weekly":
                var diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                var weekStart = now.Date.AddDays(-diff);
                return (weekStart, weekStart.AddDays(7).AddTicks(-1));

            case "monthly":
                var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                return (monthStart, monthStart.AddMonths(1).AddTicks(-1));

            case "custom":
                if (!from.HasValue || !to.HasValue)
                {
                    throw new ValidationAppException("period=custom requires both 'from' and 'to' query parameters.");
                }
                if (from.Value > to.Value)
                {
                    throw new ValidationAppException("'from' must be before 'to'.");
                }
                return (from.Value, to.Value);

            default:
                throw new ValidationAppException("period must be one of: daily, weekly, monthly, custom.");
        }
    }
}
