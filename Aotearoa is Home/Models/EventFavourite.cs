using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class EventFavourite
    {
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }

        public Event? Event { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}