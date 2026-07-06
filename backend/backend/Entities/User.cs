using backend.Entities;
using backend.Enums;
using System.ComponentModel.DataAnnotations;

public class User : BaseEntity
{
    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } = UserRole.User;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}