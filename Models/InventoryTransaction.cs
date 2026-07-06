using StoreManagementSystem.Models.Enums;

namespace StoreManagementSystem.Models;

public class InventoryTransaction
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public InventoryTransactionType Type { get; set; }
    // Positive for purchase/return, negative for sale/damaged/adjustment-down
    public int QuantityChange { get; set; }
    public int QuantityAfter { get; set; }
    public string? Note { get; set; }

    public int? RelatedSaleId { get; set; }
    public Sale? RelatedSale { get; set; }

    public int CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
