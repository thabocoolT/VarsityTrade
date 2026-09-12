using System.ComponentModel.DataAnnotations; // Provides Required and EmailAddress validation

namespace VarsityTrade.Web.Models.Auth
{
    // This view model defines the data the login form collects
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        // Error message shown when login fails
        public string? ErrorMessage { get; set; }
    }
}