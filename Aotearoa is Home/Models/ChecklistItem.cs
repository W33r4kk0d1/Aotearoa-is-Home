using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class ChecklistItem
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int ChecklistTaskId { get; set; }

        public bool IsCompleted { get; set; }

        public ApplicationUser? User { get; set; }

        public ChecklistTask? ChecklistTask { get; set; }
    }
}
