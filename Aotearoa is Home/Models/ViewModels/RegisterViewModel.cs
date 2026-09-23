using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please select an account type.")]
        public string AccountType { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required.")]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        // [Required(ErrorMessage = "Contact number is required.")]
        // [Phone(ErrorMessage = "Please enter a valid contact number.")]
        // [Display(Name = "Contact number")]
        // public string ContactNumber { get; set; } = string.Empty;

        [Phone]
        public string? ContactNumber { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The passwords do not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Preferred language")]
        public int? LanguageId { get; set; }

        // STUDENT
        public string? StudentId { get; set; }

        // ADMIN
        public string? EmployeeId { get; set; }

        public string? DepartmentOrganisation { get; set; }


        // FAMILY MEMBER
        public string? RelationshipToStudent { get; set; }

        public string? StudentReference { get; set; }


        // SERVICE PROVIDER
        public string? OrganisationName { get; set; }

        public string? OrganisationType { get; set; }

        public string? OrganisationDescription { get; set; }

        public string? OrganisationPhone { get; set; }

        public string? Website { get; set; }

        public string? OfficeAddress { get; set; }

        public string? SupportingInformation { get; set; }
    }
}