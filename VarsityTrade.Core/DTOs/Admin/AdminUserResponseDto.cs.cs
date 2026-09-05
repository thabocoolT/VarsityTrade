namespace VarsityTrade.Core.DTOs.Admin
{
    // This DTO defines what the API returns for each user in the admin panel
    // Used on the Manage Users page
    public class AdminUserResponseDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsBanned { get; set; }
        public bool StudentVerified { get; set; }
        public string? StudentNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // University info — shown in the users table
        public int UniversityId { get; set; }
        public string UniversityName { get; set; } = string.Empty;
        public string UniversityShortName { get; set; } = string.Empty;

        // Location info — suburb and res
        public string? Suburb { get; set; }
        public string? ResidenceName { get; set; }

        // Whether this user has an active seller profile
        public bool HasSellerProfile { get; set; }
        public string? StoreName { get; set; }
    }
}