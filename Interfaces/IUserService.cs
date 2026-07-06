using StoreManagementSystem.DTOs.Users;

namespace StoreManagementSystem.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync(string? search);
    Task<UserDto?> GetByIdAsync(int id);
    Task<UserDto> CreateAsync(CreateUserDto dto);
    Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto);
    Task<bool> DeleteAsync(int id);
}
