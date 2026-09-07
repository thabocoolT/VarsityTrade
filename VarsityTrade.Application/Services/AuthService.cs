using Microsoft.AspNetCore.Identity; // Provides UserManager and password hashing
using Microsoft.EntityFrameworkCore; // Provides FirstOrDefaultAsync and other EF queries
using Microsoft.Extensions.Configuration; // Provides IConfiguration for reading appsettings.json
using Microsoft.IdentityModel.Tokens; // Provides JwtSecurityToken and signing credentials
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt; // Provides JwtSecurityTokenHandler
using System.Security.Claims; // Provides Claim and ClaimTypes
using System.Security.Cryptography; // Provides RandomNumberGenerator for refresh tokens
using System.Text;
using VarsityTrade.Core.DTOs.Auth; // Provides Auth DTOs
using VarsityTrade.Core.Entities; // Provides User and RefreshToken entities
using VarsityTrade.Core.Interfaces; // Provides IAuthService interface
using VarsityTrade.Infrastructure.Data; // Provides VarsityTradeDbContext

namespace VarsityTrade.Application.Services
{
    //AuthService implements IAuthoService and handles all authentication logic
    //It uses ASP.NET Identity for user management and password hashing
    //and generates JWT tokens for stateless API authentification
    public class AuthService : IAuthService
    {
        //UserManager provides Identity methods like CreateAsync and CheckPasswordAsync
        private readonly UserManager<User> _userManager;

        //DbContext is used directly for location and refreshToken management
        private readonly VarsityTradeDbContext _context;

        //IConfiguration provides access to appsettings.json values like JWT settings
        private readonly IConfiguration _configuration;

        //Constructor receives dependencies via dependency injection
        public AuthService(
            UserManager<User> userManager,
            VarsityTradeDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _context = context;
            _configuration = configuration;

        }
        // ─────────────────────────────────────────────────────────────
        // REGISTER
        // ─────────────────────────────────────────────────────────────

        public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request)
        {
            //Check is user with this email already exists
            //We cannot have two accounts with the same email
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return null;//Email already taken-return null to signal failure

          

            //Create the location record first so we can link it to the user
            //Location is stored separately to support the 1-to-many relationship
            var location = new Location
            {
                Suburb = request.Suburb,
                City = request.City,
                Province = request.Province,
                ResidenceName = request.ResidenceName,

            };

            //Save the location to get its generated LocationID
            await _context.Locations.AddAsync(location);
            await _context.SaveChangesAsync();

            //Create the new User entity with all required fields
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.Email,
                UniversityId = request.UniversityId,
                LocationId = location.LocationId,
                StudentNumber = request.StudentNumber,
                Role = "Buyer",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,

            };

            //CreateAsync hashes the password and saves the user to AspNet Users
            var result = await _userManager.CreateAsync(user, request.Password);

            //If identity reports errors return null-registration failed
            if (!result.Succeeded) return null;

            //Generate tokens and return the aut response
            return await GenerateAuthResponseAsync(user);


        }
        // ─────────────────────────────────────────────────────────────
        // LOGIN
        // ─────────────────────────────────────────────────────────────
        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request)
        {
            // Find the user via Identity — used only for password verification
            var identityUser = await _userManager.FindByEmailAsync(request.Email);
            if (identityUser == null)
                return null;

            // Check if banned or inactive
            if (identityUser.IsBanned || !identityUser.IsActive)
                return null;

            // Verify password against the stored hash
            var passwordValid = await _userManager.CheckPasswordAsync(identityUser, request.Password);
            if (!passwordValid)
                return null;

            // Reload the user DIRECTLY from the DbContext to get the latest Role value
            // UserManager caches the user — direct context query always returns fresh data
            var freshUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == identityUser.Id);

            if (freshUser == null)
                return null;

            // Update LastLoginAt on the fresh user
            freshUser.LastLoginAt = DateTime.UtcNow;
            _context.Users.Update(freshUser);
            await _context.SaveChangesAsync();

            // Generate tokens using the fresh user — this picks up the latest Role from the database
            return await GenerateAuthResponseAsync(freshUser);
        }
        // ─────────────────────────────────────────────────────────────
        // REFRESH TOKEN
        // ─────────────────────────────────────────────────────────────
        public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            //Find the refresh token in the database
            //Include the User navigation so we can generate a new access token
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)//Load the related User
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            //Return null if the token does not exist, is expired,or has been revoked
            if (storedToken == null ||
                storedToken.ExpiresAt < DateTime.UtcNow ||
                storedToken.RevokedAt != null)
                return null;

            // Revoke the old refresh token — each token can only be used once
            // This is a security measure called token rotation
            storedToken.RevokedAt = DateTime.UtcNow;
            _context.RefreshTokens.Update(storedToken);
            await _context.SaveChangesAsync();

            //Generate a new token and return the auth response
            return await GenerateAuthResponseAsync(storedToken.User);
        }
        // ─────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────

        // Generates a JWT access token and a refresh token for the given user
        // Called after successful register, login, and token refresh
        public async Task<AuthResponseDto> GenerateAuthResponseAsync(User user)
        {
            // Generate the JWT access token
            var accessToken = GenerateJwtToken(user);

            // Generate the refresh token and save it to the database
            var refreshToken = await GenerateAndSaveRefreshTokenAsync(user);

            // Read token expiry from configuration
            var expiryMinutes = int.Parse(
                _configuration["JwtSettings:AccessTokenExpiryMinutes"] ?? "60"
            );

            // Build and return the response DTO with all token and user info
            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
                UserId = user.Id, // Identity uses Id not UserId
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Role = user.Role,
                UniversityId = user.UniversityId,
            };
        }

        // Generates a signed JWT access token containing the user's claims
        // Claims are pieces of information embedded in the token the API can read
        private string GenerateJwtToken(User user)
        {
            //Read JWT settings from appsettings.json
            var secretKey = _configuration["JwtSettings:SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey is not configured");
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];
            var expiry = int.Parse(_configuration["JwtSettings:AccessTokenExpiryMinutes"] ?? "60");


            //Create the signing key from the secret - must be at least 32 characters
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            //Claims are the data payload embedded inside the Jwt token
            //The API reads these claims to identify the user on each request
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),      // Email address
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()), // Unique token ID
                new Claim("firstName",     user.FirstName),                       // Custom claim
                new Claim("lastName",      user.LastName),                        // Custom claim
                new Claim("role",          user.Role),                            // Role claim
                new Claim("universityId",  user.UniversityId.ToString()),
            };

            //Build the JWT token with all settings
            var token = new JwtSecurityToken(

                issuer:issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiry),
                signingCredentials: creds

                );

            //Serialize the token to a string for sending to the client
            return new JwtSecurityTokenHandler().WriteToken(token);


        }
        // Generates a cryptographically secure refresh token and saves it to the database
        private async Task<string>GenerateAndSaveRefreshTokenAsync(User user)
        {
            // Generate 64 random bytes and convert to a base64 string
            // This produces a secure, unguessable token string
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var token=Convert.ToBase64String(randomBytes);

            //Read refresh token expiry from configuration
            var expiryDays = int.Parse(
                    _configuration["JwtSettings:RefreshTokenExpiryDays"]?? "7"

                );

            //Create the refreshToken entity to save in the database
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,  //Link to the user
                Token = token,  //The generated token string
                ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),//Expiry date
                RevokedAt = null,  //Not revoked yet
            };

            //Save the refresh token to the database
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return token;
        }
    }
}
