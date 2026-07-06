using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.DTOs.Employees;

public class EmployeeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; }
}

public class CreateEmployeeDto
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Position { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Salary must be greater than 0.")]
    public decimal Salary { get; set; }

    [Required]
    public DateTime HireDate { get; set; }
}

public class UpdateEmployeeDto
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Position { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Salary must be greater than 0.")]
    public decimal Salary { get; set; }

    [Required]
    public DateTime HireDate { get; set; }

    public bool IsActive { get; set; } = true;
}
