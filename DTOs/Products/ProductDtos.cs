using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.DTOs.Products;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
    public int Quantity { get; set; }
    public int LowStockThreshold { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsLowStock => Quantity <= LowStockThreshold;
}

public class CreateProductDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Sku { get; set; } = string.Empty;

    public string? Barcode { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Buy price must be greater than 0.")]
    public decimal BuyPrice { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Sell price must be greater than 0.")]
    public decimal SellPrice { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    public int LowStockThreshold { get; set; } = 10;

    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    [Required]
    public int CategoryId { get; set; }
}

public class UpdateProductDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Sku { get; set; } = string.Empty;

        public int Quantity { get; set; }

    public string? Barcode { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Buy price must be greater than 0.")]
    public decimal BuyPrice { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Sell price must be greater than 0.")]
    public decimal SellPrice { get; set; }

    public int LowStockThreshold { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    [Required]
    public int CategoryId { get; set; }
}
