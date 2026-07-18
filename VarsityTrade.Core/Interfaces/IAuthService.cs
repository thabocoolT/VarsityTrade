using System;
using System.Collections.Generic;
using System.Text;
using VarsityTrade.Core.DTOs.Auth; // Provides the Auth DTOs

namespace VarsityTrade.Core.Interfaces
{
    // This interface defines the contract for the authentication service
    // Interfaces allow us to swap implementations and make testing easier
    // The API layer depends on this interface, not the concrete implementation
    public interface IAuthService
    {
        //Registers a new user-returns a token response on success
        //Returns null if login fails registration
        Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request);

        //Logs in an existing user-returns a token response on success
        //Returns null if login fails(wrong email or password)
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);


        //Generate a new success access token using a valid refresh token
        //Returns null if the refresh token is invalid or expired
        Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken);

    }
}
