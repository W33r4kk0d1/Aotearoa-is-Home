using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class ChecklistItem
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
