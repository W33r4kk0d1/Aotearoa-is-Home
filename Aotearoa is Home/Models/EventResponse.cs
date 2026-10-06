using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class EventResponse
    {
        public int Id { get; set; }


        // EVENT
        [Required]
        public int EventId { get; set; }

        public Event? Event { get; set; }


        // USER
        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }


        // RESPONSE DATE
        public DateTime CreatedAt { get; set; }
    }
}