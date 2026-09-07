using Microsoft.AspNetCore.Authentication.JwtBearer; // Provides JWT bearer authentication
using Microsoft.AspNetCore.Identity; // Provides Identity services
using Microsoft.EntityFrameworkCore; // Provides UseSqlServer
using Microsoft.IdentityModel.Tokens; // Provides TokenValidationParameters
using System.Text; // Provides Encoding for secret key
using VarsityTrade.Application.Services; // Provides AuthService
using VarsityTrade.Core.Entities; // Provides User entity
using VarsityTrade.Core.Interfaces; // Provides IAuthService
using VarsityTrade.Infrastructure.Data; // Provides VarsityTradeDbContext and

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────
// DATABASE
// ─────────────────────────────────────────────────────────────

// Register DbContext with SQL Server — reads connection string from appsettings.json
builder.Services.AddDbContext<VarsityTradeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─────────────────────────────────────────────────────────────
// IDENTITY
// ─────────────────────────────────────────────────────────────

// Register ASP.NET Identity with our custom User entity
// AddEntityFrameworkStores connects Identity to our DbContext
// so it uses our VarsityTradeDB database for user storage
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    // Password policy — enforce strong passwords
    options.Password.RequireDigit = true;  // Must contain a number
    options.Password.RequireLowercase = true;  // Must contain lowercase letter
    options.Password.RequireUppercase = true;  // Must contain uppercase letter
    options.Password.RequireNonAlphanumeric = false; // Special chars optional
    options.Password.RequiredLength = 8;     // Minimum 8 characters

    // Lock out after 5 failed attempts for 15 minutes — prevents brute force attacks
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

    // Email must be unique across all users
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<VarsityTradeDbContext>() // Use our DbContext for storage
.AddDefaultTokenProviders(); // Adds token providers for password reset etc

// ─────────────────────────────────────────────────────────────
// JWT AUTHENTICATION
// ─────────────────────────────────────────────────────────────

// Read JWT settings from appsettings.json
var jwtSecret = builder.Configuration["JwtSettings:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey is missing from configuration");
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
var jwtAudience = builder.Configuration["JwtSettings:Audience"];

// Register JWT Bearer authentication
// This tells the API to validate incoming JWT tokens on protected endpoints
builder.Services.AddAuthentication(options =>
{
    // Set JWT Bearer as the default authentication scheme
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Prevent the JWT middleware from remapping claim names
    // Without this the role claim gets renamed and IsAdmin() cannot find it
    options.MapInboundClaims = false;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(
                                       Encoding.UTF8.GetBytes(jwtSecret))
    };
});

// ─────────────────────────────────────────────────────────────
// APPLICATION SERVICES
// ─────────────────────────────────────────────────────────────

// Register AuthService — scoped means one instance per HTTP request
builder.Services.AddScoped<IAuthService, AuthService>();
// Register ListingService — handles all listing CRUD operations
builder.Services.AddScoped<IListingService, ListingService>();

// Register SellerProfileService — handles seller profile activation and updates
builder.Services.AddScoped<ISellerProfileService, SellerProfileService>();

// Register the database seeder
builder.Services.AddScoped<VarsityTradeSeeder>();

// Register MessagingService — handles all conversation and message operations
builder.Services.AddScoped<IMessagingService, MessagingService>();

// Register OfferService — handles offer creation, acceptance, rejection, and cancellation
builder.Services.AddScoped<IOfferService, OfferService>();

// Register TransactionService — handles transaction history and review eligibility checks
builder.Services.AddScoped<ITransactionService, TransactionService>();

// Register ReviewService — handles review creation and retrieval gated by transactions
builder.Services.AddScoped<IReviewService, ReviewService>();

// Register NotificationService — handles in-app notifications across all platform events
builder.Services.AddScoped<INotificationService, NotificationService>();

// Register AdminService — handles all admin operations across users, listings, reports, and banner
builder.Services.AddScoped<IAdminService, AdminService>();
// ─────────────────────────────────────────────────────────────
// CONTROLLERS & SWAGGER
// ─────────────────────────────────────────────────────────────

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger to support JWT authentication
// This adds an Authorize button to the Swagger UI so you can test protected endpoints
builder.Services.AddSwaggerGen(options =>
{
    // Add a security definition — tells Swagger about our JWT scheme
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    // Require the Bearer token on all endpoints by default
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ─────────────────────────────────────────────────────────────
// BUILD
// ─────────────────────────────────────────────────────────────

var app = builder.Build();

// ─────────────────────────────────────────────────────────────
// SEED DATABASE ON STARTUP
// ─────────────────────────────────────────────────────────────

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<VarsityTradeSeeder>();
    await seeder.SeedAsync();
}

// ─────────────────────────────────────────────────────────────
// MIDDLEWARE PIPELINE
// ─────────────────────────────────────────────────────────────

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// UseAuthentication must come before UseAuthorization
// Authentication identifies who the user is
// Authorization checks what they are allowed to do
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();