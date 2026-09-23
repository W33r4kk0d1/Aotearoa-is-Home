using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class PendingStudentRegistration
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string? StudentId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending";
    }
}