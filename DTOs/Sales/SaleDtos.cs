using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.DTOs.Sales;

public class SaleItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class SaleDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<SaleItemDto> Items { get; set; } = new();
}

public class CreateSaleItemDto
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }
}

public class CreateSaleDto
{
    [Required, MinLength(1, ErrorMessage = "A sale must include at least one product.")]
    public List<CreateSaleItemDto> Items { get; set; } = new();
}
