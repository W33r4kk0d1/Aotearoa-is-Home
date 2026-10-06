using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class StudentFamilyMember
    {
        public int Id { get; set; }

        // Student who added this family member
        [Required]
        public string StudentUserId { get; set; } = string.Empty;

        public ApplicationUser? StudentUser { get; set; }


        // Filled when this family member successfully registers
        public string? RegisteredUserId { get; set; }

        public ApplicationUser? RegisteredUser { get; set; }


        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Relationship is required.")]
        [StringLength(50)]
        [Display(Name = "Relationship")]
        public string RelationshipToStudent { get; set; } = string.Empty;


        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(256)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;


        [StringLength(30)]
        [Display(Name = "Contact Number")]
        public string? ContactNumber { get; set; }


        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }


        [StringLength(50)]
        public string? Gender { get; set; }


        [StringLength(100)]
        [Display(Name = "Country of Citizenship")]
        public string? CountryOfCitizenship { get; set; }


        [StringLength(500)]
        public string? Notes { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}