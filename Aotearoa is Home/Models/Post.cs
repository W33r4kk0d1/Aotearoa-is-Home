
using System;
using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class CommunityPost
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [StringLength(5000)]
        public string Description { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public string Status { get; set; }
            = "Pending";

        public byte[]? ImageData { get; set; }

        public string? ImageContentType { get; set; }
    }
}
