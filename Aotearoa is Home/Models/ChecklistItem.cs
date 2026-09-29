using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class ChecklistItem
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        // ## Category for grouping checklist items
        [Required]
        public string Category { get; set; } = "Other";

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        // ## Stores whether the checklist item is completed
        public bool IsCompleted { get; set; }

        public ApplicationUser? User { get; set; }
    }
}