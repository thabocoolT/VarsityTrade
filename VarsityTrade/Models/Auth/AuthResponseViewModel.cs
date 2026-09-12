namespace VarsityTrade.Web.Models.Auth
{
    // This view model maps the API auth response to a C# object
    // Used after register and login to store the token and user info in session
    public class AuthResponseViewModel
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int UniversityId { get; set; }
    }
}