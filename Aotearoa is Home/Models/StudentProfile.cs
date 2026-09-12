using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class StudentProfile
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }
    }
}