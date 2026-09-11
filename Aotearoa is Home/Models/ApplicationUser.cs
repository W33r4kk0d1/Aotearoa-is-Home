using Microsoft.AspNetCore.Identity;

namespace Aotearoa_is_Home.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? ContactNumber { get; set; }

        public int? LanguageId { get; set; }

        public Language? Language { get; set; }
    }
}