
using System;
using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class CommunityPostFavourite
    {
        public int Id { get; set; }

        [Required]
        public int CommunityPostId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public CommunityPost? CommunityPost { get; set; }

        public ApplicationUser? User { get; set; }
    }
}
