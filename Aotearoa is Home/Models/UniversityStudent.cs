using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class UniversityStudent
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string StudentEmail { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(200)]
        public string? ApplicationEmail { get; set; }

        public bool IsCurrentStudent { get; set; }
    }
}