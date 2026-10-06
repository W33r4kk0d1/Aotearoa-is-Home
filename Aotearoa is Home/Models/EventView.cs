namespace Aotearoa_is_Home.Models
{
    public class EventView
    {
        public int Id { get; set; }

        // EVENT
        public int EventId { get; set; }

        public Event? Event { get; set; }


        // USER
        public string? UserId { get; set; }

        public ApplicationUser? User { get; set; }


        // VIEW DATE
        public DateTime ViewedAt { get; set; }
    }
}