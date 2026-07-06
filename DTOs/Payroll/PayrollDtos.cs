using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.DTOs.Payroll;

public class PayrollDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }
    public decimal Bonus { get; set; }
    public decimal Deduction { get; set; }
    public decimal NetSalary { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GeneratePayrollDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Bonus { get; set; } = 0;

    [Range(0, double.MaxValue)]
    public decimal Deduction { get; set; } = 0;

    [Range(1, 12)]
    public int Month { get; set; }

    [Range(2000, 2100)]
    public int Year { get; set; }
}
