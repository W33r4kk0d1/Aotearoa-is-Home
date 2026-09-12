using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class EventProviderProfile
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string OrganisationName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string OrganisationType { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? OrganisationDescription { get; set; }

        [StringLength(30)]
        public string? OrganisationPhone { get; set; }

        [Url]
        [StringLength(300)]
        public string? Website { get; set; }

        [StringLength(500)]
        public string? OfficeAddress { get; set; }

        [StringLength(1000)]
        public string? SupportingInformation { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }
    }
}