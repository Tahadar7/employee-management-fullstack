using System.ComponentModel.DataAnnotations;

namespace backend.Entities
{
    public class RefreshToken : BaseEntity
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;

        // Convenience helper (computed, not stored)
        public bool IsActive => !IsRevoked && ExpiresAt > DateTime.UtcNow;
    }
}
