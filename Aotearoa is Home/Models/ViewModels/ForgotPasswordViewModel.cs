using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}