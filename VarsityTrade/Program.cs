using VarsityTrade.Web.Services; // Provides ApiService

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────
// SERVICES
// ─────────────────────────────────────────────────────────────

// Register MVC with controllers and Razor views
builder.Services.AddControllersWithViews();

// Register IHttpContextAccessor — needed by ApiService to read the session token
builder.Services.AddHttpContextAccessor();

// Register HttpClient for ApiService — manages connection pooling efficiently
builder.Services.AddHttpClient<ApiService>();

// Register ApiService as a scoped service — one instance per HTTP request
builder.Services.AddScoped<ApiService>();

// Register session — used to store the JWT token and user info after login
// Session is stored server-side and identified by a cookie
builder.Services.AddSession(options =>
{
    // Session expires after 60 minutes of inactivity
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    // HttpOnly prevents JavaScript from accessing the session cookie — security measure
    options.Cookie.HttpOnly = true;
    // IsEssential means the cookie is set even if the user has not consented to cookies
    options.Cookie.IsEssential = true;
});

// Register cookie authentication — for MVC page protection
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        // Redirect to login page when an unauthenticated user tries to access a protected page
        options.LoginPath = "/auth/login";
        // Redirect to access denied page when an authenticated user lacks permission
        options.AccessDeniedPath = "/auth/login";
        // Cookie expires after 7 days — matches the JWT refresh token expiry
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

// ─────────────────────────────────────────────────────────────
// BUILD
// ─────────────────────────────────────────────────────────────

var app = builder.Build();

// ─────────────────────────────────────────────────────────────
// MIDDLEWARE PIPELINE
// ─────────────────────────────────────────────────────────────

if (!app.Environment.IsDevelopment())
{
    // Show a friendly error page in production
    app.UseExceptionHandler("/Home/Error");
    // Use HSTS for HTTPS in production
    app.UseHsts();
}

// Redirect HTTP to HTTPS
app.UseHttpsRedirection();

// Serve static files — CSS, JS, images from wwwroot
app.UseStaticFiles();

// Enable routing
app.UseRouting();

// Enable session BEFORE authentication and authorisation
// Session must be available when the auth middleware reads the token
app.UseSession();

// Enable cookie authentication middleware
app.UseAuthentication();

// Enable authorisation middleware
app.UseAuthorization();

// Map controller routes — default route goes to Home/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();