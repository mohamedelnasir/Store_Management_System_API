using StoreManagementSystem.Models.Enums;

namespace StoreManagementSystem.Models;

public class Expense
{
    public int Id { get; set; }
    public ExpenseCategory Category { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;

    public int CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
