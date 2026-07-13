using Microsoft.EntityFrameworkCore; // Provides UseSqlServer and EF Core services
using VarsityTrade.Infrastructure.Data; // Provides VarsityTradeDbContext and VarsityTradeSeeder

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────
// SERVICES
// Register all services before building the app
// ─────────────────────────────────────────────────────────────

// Register the DbContext with SQL Server
// The connection string is read from appsettings.json
// This makes the DbContext available throughout the application via dependency injection
builder.Services.AddDbContext<VarsityTradeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register the seeder as a scoped service
// Scoped means a new instance is created per request — correct for database operations
builder.Services.AddScoped<VarsityTradeSeeder>();

// Register controllers — scans for all classes inheriting from ControllerBase
builder.Services.AddControllers();

// Register Swagger for API documentation and testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ─────────────────────────────────────────────────────────────
// BUILD
// ─────────────────────────────────────────────────────────────

var app = builder.Build();

// ─────────────────────────────────────────────────────────────
// SEED
// Run the seeder on startup before any requests are handled
// CreateScope creates a temporary DI scope to resolve scoped services
// ─────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    // Resolve the seeder from the DI container
    var seeder = scope.ServiceProvider.GetRequiredService<VarsityTradeSeeder>();

    // Run the seed method — inserts data only if tables are empty
    await seeder.SeedAsync();
}

// ─────────────────────────────────────────────────────────────
// MIDDLEWARE PIPELINE
// Order matters — each middleware runs in the order it is added
// ─────────────────────────────────────────────────────────────

// Enable Swagger UI only in development — not in production
if (app.Environment.IsDevelopment())
{
    // Swagger generates the JSON spec at /swagger/v1/swagger.json
    app.UseSwagger();

    // SwaggerUI serves the interactive documentation page at /swagger
    app.UseSwaggerUI();
}

// Redirect all HTTP requests to HTTPS for security
app.UseHttpsRedirection();

// Enable the authorisation middleware — required even before we add auth in Step 4.4
app.UseAuthorization();

// Map all controller routes automatically based on route attributes
app.MapControllers();

// Start the application and begin listening for requests
app.Run();