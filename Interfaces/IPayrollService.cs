using StoreManagementSystem.DTOs.Payroll;

namespace StoreManagementSystem.Interfaces;

public interface IPayrollService
{
    Task<IEnumerable<PayrollDto>> GetAllAsync(int? employeeId, int? month, int? year);
    Task<PayrollDto?> GetByIdAsync(int id);
    Task<PayrollDto> GenerateAsync(GeneratePayrollDto dto);
}
