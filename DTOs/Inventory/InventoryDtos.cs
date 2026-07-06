using System.ComponentModel.DataAnnotations;
using StoreManagementSystem.Models.Enums;

namespace StoreManagementSystem.DTOs.Inventory;

public class InventoryTransactionDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int QuantityChange { get; set; }
    public int QuantityAfter { get; set; }
    public string? Note { get; set; }
    public int? RelatedSaleId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Used for manual stock movements: Purchase, Return, Adjustment, Damaged.
/// "Sale" transactions are created automatically by the Sales module and cannot be posted here.
/// </summary>
public class CreateInventoryTransactionDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    public InventoryTransactionType Type { get; set; }

    /// <summary>
    /// Positive to add stock (Purchase, Return), negative to remove stock (Damaged, or a downward Adjustment).
    /// Cannot be zero.
    /// </summary>
    [Required]
    public int QuantityChange { get; set; }

    public string? Note { get; set; }
}

public class LowStockProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int LowStockThreshold { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}
