using StoreManagementSystem.DTOs.Sales;

namespace StoreManagementSystem.Interfaces;

public interface ISaleService
{
    Task<IEnumerable<SaleDto>> GetAllAsync(DateTime? from, DateTime? to, string? invoiceNumber);
    Task<SaleDto?> GetByIdAsync(int id);
    Task<SaleDto> CreateAsync(CreateSaleDto dto, int userId);
}
