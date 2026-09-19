using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string AccountType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Phone]
        public string? ContactNumber { get; set; }

        public int? LanguageId { get; set; }

        // Student
        public string? StudentId { get; set; }

        // Admin
        public string? EmployeeId { get; set; }

        public string? DepartmentOrganisation { get; set; }

        // Family
        public string? RelationshipToStudent { get; set; }

        public string? StudentReference { get; set; }

        // Event Provider
        public string? OrganisationName { get; set; }

        public string? OrganisationType { get; set; }

        public string? OrganisationDescription { get; set; }

        public string? OrganisationPhone { get; set; }

        public string? Website { get; set; }

        public string? OfficeAddress { get; set; }

        public string? SupportingInformation { get; set; }
    }
}