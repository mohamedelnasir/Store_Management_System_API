using System.ComponentModel.DataAnnotations;
using StoreManagementSystem.Models.Enums;

namespace StoreManagementSystem.DTOs.Users;

public class CreateUserDto
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

public class UpdateUserDto
{
    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    // Optional: only set when the caller wants to change the password
    [MinLength(8)]
    public string? Password { get; set; }
}
