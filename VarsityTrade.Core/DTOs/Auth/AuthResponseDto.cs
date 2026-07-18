using System;
using System.Collections.Generic;
using System.Text;

namespace VarsityTrade.Core.DTOs.Auth
{
    //This DTO defines what the API returns after a successful register or login
    //The client stores the AccessToken and uses it in the Authorization header
    //for all subsequent requests
    public class AuthResponseDto
    {
        //The JWT access token-short lived, used to authenticate API requests
        //Client sends this in the Authorization: Bearer {token} header
        public string AccessToken { get; set; } = string.Empty;

        //The refresh token-long lived, used to get a new access token
        //when the access token expires without requiring the user to log in again
        public string RefreshToken {  get; set; } = string.Empty;

        //when the access token expires-client uses this to know when to refresh
        public DateTime ExpiresAt { get; set; }


        //Basic user info returned so the client can display the user's name
        //and redirect them to the correct dashboard without an extra API call
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; }= string.Empty;
        public string Email {  get; set; } = string.Empty;
        public string Role {  get; set; } = string.Empty;
        public int UniversityId { get; set; }
    }
}
