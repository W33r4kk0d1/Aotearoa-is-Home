using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? ContactNumber { get; set; }

        public int? LanguageId { get; set; }

        [StringLength(500)]
        public string? LinkedInProfile { get; set; }

        public DateTime? CreatedAt { get; set; }
        public Language? Language { get; set; }
    }
}