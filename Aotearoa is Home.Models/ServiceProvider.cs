using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class ServiceProvider
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string BusinessName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? Website { get; set; }

        public byte[]? Logo { get; set; }

        public string? LogoContentType { get; set; }

        public List<Event> Events { get; set; } = new List<Event>();
    }
}