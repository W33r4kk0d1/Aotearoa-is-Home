using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class AdminProfile
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string EmployeeId { get; set; } = string.Empty;

        [StringLength(150)]
        public string? DepartmentOrganisation { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }
    }
}