using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagementSystem.DTOs.Payroll;
using StoreManagementSystem.Interfaces;
using StoreManagementSystem.Middleware;

namespace StoreManagementSystem.Controllers;

[ApiController]
[Route("api/payroll")]
[Authorize(Roles = "Admin")]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _payrollService;

    public PayrollController(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PayrollDto>>> GetAll(
        [FromQuery] int? employeeId,
        [FromQuery] int? month,
        [FromQuery] int? year)
    {
        return Ok(await _payrollService.GetAllAsync(employeeId, month, year));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PayrollDto>> GetById(int id)
    {
        var payroll = await _payrollService.GetByIdAsync(id);
        if (payroll is null)
        {
            throw new NotFoundException($"Payroll record with id {id} was not found.");
        }
        return Ok(payroll);
    }

    /// <summary>Generate a payroll record for an employee for a given month/year. Net Salary = Salary + Bonus - Deduction.</summary>
    [HttpPost("generate")]
    public async Task<ActionResult<PayrollDto>> Generate([FromBody] GeneratePayrollDto dto)
    {
        var payroll = await _payrollService.GenerateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = payroll.Id }, payroll);
    }
}
