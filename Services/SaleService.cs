using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Data;
using StoreManagementSystem.DTOs.Sales;
using StoreManagementSystem.Interfaces;
using StoreManagementSystem.Middleware;
using StoreManagementSystem.Models;
using StoreManagementSystem.Models.Enums;

namespace StoreManagementSystem.Services;

public class SaleService : ISaleService
{
    private readonly ApplicationDbContext _context;

    public SaleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SaleDto>> GetAllAsync(DateTime? from, DateTime? to, string? invoiceNumber)
    {
        var query = _context.Sales
            .Include(s => s.CreatedByUser)
            .Include(s => s.SaleItems).ThenInclude(si => si.Product)
            .AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(s => s.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(s => s.CreatedAt <= to.Value);
        }

        if (!string.IsNullOrWhiteSpace(invoiceNumber))
        {
            query = query.Where(s => s.InvoiceNumber.Contains(invoiceNumber));
        }

        var sales = await query.OrderByDescending(s => s.CreatedAt).ToListAsync();
        return sales.Select(ToDto);
    }

    public async Task<SaleDto?> GetByIdAsync(int id)
    {
        var sale = await _context.Sales
            .Include(s => s.CreatedByUser)
            .Include(s => s.SaleItems).ThenInclude(si => si.Product)
            .FirstOrDefaultAsync(s => s.Id == id);

        return sale is null ? null : ToDto(sale);
    }

    public async Task<SaleDto> CreateAsync(CreateSaleDto dto, int userId)
    {
        if (dto.Items is null || dto.Items.Count == 0)
        {
            throw new ValidationAppException("A sale must include at least one product.");
        }

        // Merge duplicate product entries so the same product listed twice doesn't double-check stock incorrectly.
        var mergedItems = dto.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(i => i.Quantity) })
            .ToList();

        var productIds = mergedItems.Select(i => i.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        // 1. Check stock (and existence) for every line first, before making any changes.
        foreach (var item in mergedItems)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
            {
                throw new NotFoundException($"Product with id {item.ProductId} was not found.");
            }

            if (!product.IsActive)
            {
                throw new ValidationAppException($"Product '{product.Name}' is not active and cannot be sold.");
            }

            if (product.Quantity < item.Quantity)
            {
                throw new ValidationAppException(
                    $"Insufficient stock for '{product.Name}'. Available: {product.Quantity}, requested: {item.Quantity}.");
            }
        }

        // 2. Calculate totals.
        var sale = new Sale
        {
            InvoiceNumber = GenerateInvoiceNumber(),
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        decimal total = 0;

        foreach (var item in mergedItems)
        {
            var product = products[item.ProductId];
            var lineTotal = product.SellPrice * item.Quantity;
            total += lineTotal;

            // 3. Save SaleItems (attached via navigation, persisted together with the Sale).
            sale.SaleItems.Add(new SaleItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.SellPrice,
                LineTotal = lineTotal
            });

            // 4. Reduce stock.
            product.Quantity -= item.Quantity;
            product.UpdatedAt = DateTime.UtcNow;

            // 5. Create InventoryTransaction for the sale.
            _context.InventoryTransactions.Add(new InventoryTransaction
            {
                ProductId = product.Id,
                Type = InventoryTransactionType.Sale,
                QuantityChange = -item.Quantity,
                QuantityAfter = product.Quantity,
                Note = $"Sold via invoice {sale.InvoiceNumber}",
                RelatedSale = sale,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            });
        }

        sale.Total = total;

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        var saved = await _context.Sales
            .Include(s => s.CreatedByUser)
            .Include(s => s.SaleItems).ThenInclude(si => si.Product)
            .FirstAsync(s => s.Id == sale.Id);

        return ToDto(saved);
    }

    private static string GenerateInvoiceNumber()
    {
        // Timestamp + short random suffix keeps this collision-safe without needing a DB round-trip
        // to compute a sequential counter under concurrent sales.
        var suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}-{suffix}";
    }

    private static SaleDto ToDto(Sale s) => new()
    {
        Id = s.Id,
        InvoiceNumber = s.InvoiceNumber,
        Total = s.Total,
        CreatedByUserName = s.CreatedByUser?.FullName ?? string.Empty,
        CreatedAt = s.CreatedAt,
        Items = s.SaleItems.Select(si => new SaleItemDto
        {
            ProductId = si.ProductId,
            ProductName = si.Product?.Name ?? string.Empty,
            Quantity = si.Quantity,
            UnitPrice = si.UnitPrice,
            LineTotal = si.LineTotal
        }).ToList()
    };
}
