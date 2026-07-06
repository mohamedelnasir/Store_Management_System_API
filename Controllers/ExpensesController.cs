using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagementSystem.DTOs.Expenses;
using StoreManagementSystem.Interfaces;
using StoreManagementSystem.Middleware;

namespace StoreManagementSystem.Controllers;

[ApiController]
[Route("api/expenses")]
[Authorize(Roles = "Admin,Manager")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetAll([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        return Ok(await _expenseService.GetAllAsync(from, to));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExpenseDto>> GetById(int id)
    {
        var expense = await _expenseService.GetByIdAsync(id);
        if (expense is null)
        {
            throw new NotFoundException($"Expense with id {id} was not found.");
        }
        return Ok(expense);
    }

    [HttpGet("monthly-summary")]
    public async Task<ActionResult<MonthlyExpenseSummaryDto>> GetMonthlySummary([FromQuery] int year, [FromQuery] int month)
    {
        if (month is < 1 or > 12)
        {
            throw new ValidationAppException("Month must be between 1 and 12.");
        }
        return Ok(await _expenseService.GetMonthlySummaryAsync(year, month));
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create([FromBody] CreateExpenseDto dto)
    {
        var userId = GetUserId();
        var expense = await _expenseService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = expense.Id }, expense);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ExpenseDto>> Update(int id, [FromBody] UpdateExpenseDto dto)
    {
        var expense = await _expenseService.UpdateAsync(id, dto);
        if (expense is null)
        {
            throw new NotFoundException($"Expense with id {id} was not found.");
        }
        return Ok(expense);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _expenseService.DeleteAsync(id);
        if (!deleted)
        {
            throw new NotFoundException($"Expense with id {id} was not found.");
        }
        return NoContent();
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
