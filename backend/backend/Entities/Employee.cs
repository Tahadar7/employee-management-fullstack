using System.ComponentModel.DataAnnotations;

namespace backend.Entities
{
    public class Employee : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }
    }
}
