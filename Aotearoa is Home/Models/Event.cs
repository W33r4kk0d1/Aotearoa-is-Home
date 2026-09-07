using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        public byte[]? ImageData { get; set; }

        public string? ImageContentType { get; set; }

        public int ServiceProviderId { get; set; }

        public ServiceProvider? ServiceProvider { get; set; }
    }
}