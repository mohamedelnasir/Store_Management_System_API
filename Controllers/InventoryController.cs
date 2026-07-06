using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagementSystem.DTOs.Inventory;
using StoreManagementSystem.Interfaces;
using StoreManagementSystem.Middleware;

namespace StoreManagementSystem.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize(Roles = "Admin,Manager")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    /// <summary>View inventory history, optionally filtered to a single product.</summary>
    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<InventoryTransactionDto>>> GetHistory([FromQuery] int? productId)
    {
        return Ok(await _inventoryService.GetHistoryAsync(productId));
    }

    /// <summary>Products at or below their low-stock threshold.</summary>
    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<LowStockProductDto>>> GetLowStock()
    {
        return Ok(await _inventoryService.GetLowStockAsync());
    }

    /// <summary>Record a manual stock movement: Purchase, Return, Adjustment, or Damaged.</summary>
    [HttpPost("adjust")]
    public async Task<ActionResult<InventoryTransactionDto>> Adjust([FromBody] CreateInventoryTransactionDto dto)
    {
        var userId = GetUserId();
        var result = await _inventoryService.RecordTransactionAsync(dto, userId);
        return Ok(result);
    }

    private int GetUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (idClaim is null || !int.TryParse(idClaim, out var id))
        {
            throw new UnauthorizedAppException("Unable to determine the current user from the token.");
        }
        return id;
    }
}
