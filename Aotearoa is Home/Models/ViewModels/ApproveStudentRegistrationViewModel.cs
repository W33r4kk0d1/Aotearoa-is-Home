using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models.ViewModels
{
    public class ApproveStudentRegistrationViewModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Student ID is required before approval.")]
        [Display(Name = "Student ID")]
        public string StudentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "A temporary password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Temporary Password")]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "The temporary password must be at least 8 characters.")]
        public string TemporaryPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm the temporary password.")]
        [DataType(DataType.Password)]
        [Compare(
            "TemporaryPassword",
            ErrorMessage = "The passwords do not match.")]
        [Display(Name = "Confirm Temporary Password")]
        public string ConfirmTemporaryPassword { get; set; } = string.Empty;
    }
}