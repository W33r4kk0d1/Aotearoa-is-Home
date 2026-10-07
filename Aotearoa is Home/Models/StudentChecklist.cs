using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class StudentChecklist
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsArchived { get; set; } = false;

        public ICollection<ChecklistItem> Items { get; set; }
            = new List<ChecklistItem>();
    }
}