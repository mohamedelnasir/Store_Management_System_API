using StoreManagementSystem.Models;

namespace StoreManagementSystem.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
