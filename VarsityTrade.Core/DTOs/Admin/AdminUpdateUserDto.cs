namespace VarsityTrade.Core.DTOs.Admin
{
    // This DTO defines the data admin can update on a user account
    // Admin can ban, unban, activate, or deactivate any user
    public class AdminUpdateUserDto
    {
        // IsActive controls whether the user can log in
        public bool IsActive { get; set; }

        // IsBanned permanently blocks the user from the platform
        public bool IsBanned { get; set; }

        // StudentVerified marks the student as a verified university student
        public bool StudentVerified { get; set; }
    }
}