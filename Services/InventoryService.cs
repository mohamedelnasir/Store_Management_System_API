using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Data;
using StoreManagementSystem.DTOs.Inventory;
using StoreManagementSystem.Interfaces;
using StoreManagementSystem.Middleware;
using StoreManagementSystem.Models;
using StoreManagementSystem.Models.Enums;

namespace StoreManagementSystem.Services;

public class InventoryService : IInventoryService
{
    private readonly ApplicationDbContext _context;

    public InventoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<InventoryTransactionDto>> GetHistoryAsync(int? productId)
    {
        var query = _context.InventoryTransactions
            .Include(t => t.Product)
            .Include(t => t.CreatedByUser)
            .AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(t => t.ProductId == productId.Value);
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => ToDto(t))
            .ToListAsync();
    }

    public async Task<InventoryTransactionDto> RecordTransactionAsync(CreateInventoryTransactionDto dto, int userId)
    {
        if (dto.Type == InventoryTransactionType.Sale)
        {
            throw new ValidationAppException("Sale transactions are created automatically when a sale is made and cannot be posted manually.");
        }

        if (dto.QuantityChange == 0)
        {
            throw new ValidationAppException("Quantity change cannot be zero.");
        }

        // Purchases and returns should always add stock; damaged goods should always remove stock.
        if (dto.Type is InventoryTransactionType.Purchase or InventoryTransactionType.Return && dto.QuantityChange < 0)
        {
            throw new ValidationAppException($"{dto.Type} transactions must have a positive quantity change.");
        }

        if (dto.Type == InventoryTransactionType.Damaged && dto.QuantityChange > 0)
        {
            throw new ValidationAppException("Damaged transactions must have a negative quantity change.");
        }

        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product is null)
        {
            throw new NotFoundException($"Product with id {dto.ProductId} was not found.");
        }

        var newQuantity = product.Quantity + dto.QuantityChange;
        if (newQuantity < 0)
        {
            throw new ValidationAppException($"This adjustment would bring stock below zero. Current stock: {product.Quantity}.");
        }

        product.Quantity = newQuantity;
        product.UpdatedAt = DateTime.UtcNow;

        var transaction = new InventoryTransaction
        {
            ProductId = product.Id,
            Type = dto.Type,
            QuantityChange = dto.QuantityChange,
            QuantityAfter = newQuantity,
            Note = dto.Note,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.InventoryTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        await _context.Entry(transaction).Reference(t => t.Product).LoadAsync();
        await _context.Entry(transaction).Reference(t => t.CreatedByUser).LoadAsync();

        return ToDto(transaction);
    }

    public async Task<IEnumerable<LowStockProductDto>> GetLowStockAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.Quantity <= p.LowStockThreshold)
            .OrderBy(p => p.Quantity)
            .Select(p => new LowStockProductDto
            {
                ProductId = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                Quantity = p.Quantity,
                LowStockThreshold = p.LowStockThreshold,
                CategoryName = p.Category!.Name
            })
            .ToListAsync();
    }

    private static InventoryTransactionDto ToDto(InventoryTransaction t) => new()
    {
        Id = t.Id,
        ProductId = t.ProductId,
        ProductName = t.Product?.Name ?? string.Empty,
        Type = t.Type.ToString(),
        QuantityChange = t.QuantityChange,
        QuantityAfter = t.QuantityAfter,
        Note = t.Note,
        RelatedSaleId = t.RelatedSaleId,
        CreatedByUserName = t.CreatedByUser?.FullName ?? string.Empty,
        CreatedAt = t.CreatedAt
    };
}
