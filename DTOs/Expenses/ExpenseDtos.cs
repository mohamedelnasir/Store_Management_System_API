using System.ComponentModel.DataAnnotations;
using StoreManagementSystem.Models.Enums;

namespace StoreManagementSystem.DTOs.Expenses;

public class ExpenseDto
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Note { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateExpenseDto
{
    [Required]
    public ExpenseCategory Category { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    public string? Note { get; set; }

    public DateTime? ExpenseDate { get; set; }
}

public class UpdateExpenseDto
{
    [Required]
    public ExpenseCategory Category { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    public string? Note { get; set; }

    [Required]
    public DateTime ExpenseDate { get; set; }
}

public class MonthlyExpenseSummaryDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Total { get; set; }
    public List<CategoryExpenseTotalDto> ByCategory { get; set; } = new();
}

public class CategoryExpenseTotalDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
