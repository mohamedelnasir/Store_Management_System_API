using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Data;
using StoreManagementSystem.DTOs.Products;
using StoreManagementSystem.Interfaces;
using StoreManagementSystem.Middleware;
using StoreManagementSystem.Models;

namespace StoreManagementSystem.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(string? search, int? categoryId, bool? lowStockOnly)
    {
        var query = _context.Products.Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.Name.Contains(search) ||
                p.Sku.Contains(search) ||
                (p.Barcode != null && p.Barcode.Contains(search)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (lowStockOnly == true)
        {
            query = query.Where(p => p.Quantity <= p.LowStockThreshold);
        }

        return await query
            .OrderBy(p => p.Name)
            .Select(p => ToDto(p))
            .ToListAsync();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        return product is null ? null : ToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        ValidatePricing(dto.BuyPrice, dto.SellPrice);

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
        {
            throw new ValidationAppException("The specified category does not exist.");
        }

        var skuExists = await _context.Products.AnyAsync(p => p.Sku == dto.Sku);
        if (skuExists)
        {
            throw new ConflictException("A product with this SKU already exists.");
        }

        var product = new Product
        {
            Name = dto.Name,
            Sku = dto.Sku,
            Barcode = dto.Barcode,
            BuyPrice = dto.BuyPrice,
            SellPrice = dto.SellPrice,
            Quantity = dto.Quantity,
            LowStockThreshold = dto.LowStockThreshold,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            CategoryId = dto.CategoryId,
            IsActive = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        await _context.Entry(product).Reference(p => p.Category).LoadAsync();
        return ToDto(product);
    }

    public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
        {
            return null;
        }

        ValidatePricing(dto.BuyPrice, dto.SellPrice);

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
        {
            throw new ValidationAppException("The specified category does not exist.");
        }

        var skuTaken = await _context.Products.AnyAsync(p => p.Sku == dto.Sku && p.Id != id);
        if (skuTaken)
        {
            throw new ConflictException("Another product already uses this SKU.");
        }

        product.Name = dto.Name;
        product.Sku = dto.Sku;
        product.Barcode = dto.Barcode;
        product.BuyPrice = dto.BuyPrice;
        product.Quantity=dto.Quantity;
        product.SellPrice = dto.SellPrice;
        product.LowStockThreshold = dto.LowStockThreshold;
        product.Description = dto.Description;
        product.ImageUrl = dto.ImageUrl;
        product.CategoryId = dto.CategoryId;
        product.IsActive = dto.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _context.Entry(product).Reference(p => p.Category).LoadAsync();

        return ToDto(product);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    private static void ValidatePricing(decimal buyPrice, decimal sellPrice)
    {
        if (sellPrice <= buyPrice)
        {
            throw new ValidationAppException("Sell price must be greater than buy price.");
        }
    }

    private static ProductDto ToDto(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Sku = p.Sku,
        Barcode = p.Barcode,
        BuyPrice = p.BuyPrice,
        SellPrice = p.SellPrice,
        Quantity = p.Quantity,
        LowStockThreshold = p.LowStockThreshold,
        Description = p.Description,
        ImageUrl = p.ImageUrl,
        IsActive = p.IsActive,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.Name ?? string.Empty
    };
}
