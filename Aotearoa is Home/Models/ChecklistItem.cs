using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class ChecklistItem
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        // The checklist journey this item belongs to
        public int? StudentChecklistId { get; set; }

        public StudentChecklist? StudentChecklist { get; set; }

        [Required]
        public string Category { get; set; } = "Other";

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsCompleted { get; set; }

        public int? ChecklistTaskId { get; set; }

        public ChecklistTask? ChecklistTask { get; set; }
    }
}