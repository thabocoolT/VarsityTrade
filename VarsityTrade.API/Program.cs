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
// Configure Swagger with full API documentation
builder.Services.AddSwaggerGen(options =>
{
    // API metadata — shown at the top of the Swagger UI page
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Varsity Trade API",
        Version = "v1",
        Description = "REST API for Varsity Trade — a university-locked student marketplace for South Africa. " +
                      "All authenticated endpoints require a Bearer JWT token. " +
                      "Listing feeds are campus-locked — students only see listings from their own university.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Thabo Motau",
            Email = "thabo@varsitytrade.co.za",
        }


    });

    // JWT Bearer authentication definition
    // This adds the Authorize button to the Swagger UI
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token in this format: Bearer {your token here}\n\n" +
                      "Get a token by calling POST /api/auth/login or POST /api/auth/register."
    });

    // Require Bearer token on all endpoints by default
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

    // Group endpoints by controller name for cleaner navigation
    options.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
    // Include XML comments in Swagger UI — reads from the generated documentation file
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (System.IO.File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
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
// ─────────────────────────────────────────────────────────────
// SEED DATABASE ON STARTUP
// ─────────────────────────────────────────────────────────────
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider
                          .GetRequiredService<VarsityTradeDbContext>();
    var userManager = scope.ServiceProvider
                          .GetRequiredService<UserManager<User>>();

    // Apply any pending migrations automatically
    await context.Database.MigrateAsync();

    // Seed universities, categories, conditions, statuses, settings
    var seeder = new VarsityTradeSeeder(context, userManager);
    await seeder.SeedAsync();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Database seeding failed. App will continue.");
}

app.Run();