using System.ComponentModel.DataAnnotations;
using StoreManagementSystem.Models.Enums;

namespace StoreManagementSystem.DTOs.Auth;

public class RegisterDto
{
    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }
}
