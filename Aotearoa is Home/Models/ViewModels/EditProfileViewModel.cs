using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models.ViewModels
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Contact Number")]
        public string? ContactNumber { get; set; }

        [Display(Name = "Language")]
        public int? LanguageId { get; set; }

        [Display(Name = "LinkedIn Profile")]
        [Url(ErrorMessage = "Please enter a valid LinkedIn URL.")]
        public string? LinkedInProfile { get; set; }
    }
}