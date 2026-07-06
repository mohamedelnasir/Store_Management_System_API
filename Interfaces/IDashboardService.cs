using StoreManagementSystem.DTOs.Dashboard;

namespace StoreManagementSystem.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
}
