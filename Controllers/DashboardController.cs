using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagementSystem.DTOs.Dashboard;
using StoreManagementSystem.Interfaces;

namespace StoreManagementSystem.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Admin,Manager")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        return Ok(await _dashboardService.GetSummaryAsync());
    }
}
