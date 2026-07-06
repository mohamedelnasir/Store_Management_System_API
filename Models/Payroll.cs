namespace StoreManagementSystem.Models;

public class Payroll
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public decimal BaseSalary { get; set; }
    public decimal Bonus { get; set; }
    public decimal Deduction { get; set; }
    public decimal NetSalary { get; set; }

    public int Month { get; set; }
    public int Year { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
