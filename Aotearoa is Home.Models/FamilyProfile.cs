using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class FamilyProfile
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string RelationshipToStudent { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string StudentReference { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }
    }
}