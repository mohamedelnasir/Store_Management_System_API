using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagementSystem.DTOs.Sales;
using StoreManagementSystem.Interfaces;
using StoreManagementSystem.Middleware;

namespace StoreManagementSystem.Controllers;

[ApiController]
[Route("api/sales")]
[Authorize(Roles = "Admin,Manager,Cashier")]
public class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;

    public SalesController(ISaleService saleService)
    {
        _saleService = saleService;
    }

    /// <summary>Create a sale: checks stock, calculates totals, saves the invoice, and reduces stock.</summary>
    [HttpPost]
    public async Task<ActionResult<SaleDto>> Create([FromBody] CreateSaleDto dto)
    {
        var userId = GetUserId();
        var sale = await _saleService.CreateAsync(dto, userId);
        // Not using CreatedAtAction(nameof(GetById)) here since GetById is Admin/Manager-only
        // and a Cashier who just created the sale wouldn't be able to follow that Location header.
        return StatusCode(StatusCodes.Status201Created, sale);
    }

    /// <summary>View sales history, optionally filtered by date range or invoice number.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<SaleDto>>> GetAll(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? invoiceNumber)
    {
        return Ok(await _saleService.GetAllAsync(from, to, invoiceNumber));
    }

    /// <summary>View a single invoice. Admin/Manager can view any invoice.</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<SaleDto>> GetById(int id)
    {
        var sale = await _saleService.GetByIdAsync(id);
        if (sale is null)
        {
            throw new NotFoundException($"Sale with id {id} was not found.");
        }
        return Ok(sale);
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
