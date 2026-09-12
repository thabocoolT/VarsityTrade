using System.ComponentModel.DataAnnotations; // Provides Required, EmailAddress, MinLength validation

namespace VarsityTrade.Web.Models.Auth
{
    // This view model defines the data the register form collects from the student
    // It mirrors RegisterRequestDto in the API but is designed for MVC form binding
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "University is required")]
        [Display(Name = "University")]
        public int UniversityId { get; set; }

        [Required(ErrorMessage = "Suburb is required")]
        [Display(Name = "Suburb")]
        public string Suburb { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Province is required")]
        [Display(Name = "Province")]
        public string Province { get; set; } = string.Empty;

        [Display(Name = "Residence name")]
        public string? ResidenceName { get; set; }

        [Display(Name = "Student number")]
        public string? StudentNumber { get; set; }

        // List of universities for the dropdown — populated from the API
        public List<UniversityOption> Universities { get; set; } = new();
    }

    // Simple option class for the university dropdown
    public class UniversityOption
    {
        public int UniversityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
    }
}