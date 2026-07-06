using StoreManagementSystem.DTOs.Inventory;

namespace StoreManagementSystem.Interfaces;

public interface IInventoryService
{
    Task<IEnumerable<InventoryTransactionDto>> GetHistoryAsync(int? productId);
    Task<InventoryTransactionDto> RecordTransactionAsync(CreateInventoryTransactionDto dto, int userId);
    Task<IEnumerable<LowStockProductDto>> GetLowStockAsync();
}
