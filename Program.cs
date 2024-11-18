using System.Text;
using Edusync.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Set up Serilog configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
    .WriteTo.File("Logs/AllLogs.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.File("Logs/InformationLogs.txt", restrictedToMinimumLevel: LogEventLevel.Information, rollingInterval: RollingInterval.Day)
    .WriteTo.File("Logs/WarningLogs.txt", restrictedToMinimumLevel: LogEventLevel.Warning, rollingInterval: RollingInterval.Day)
    .WriteTo.File("Logs/ErrorLogs.txt", restrictedToMinimumLevel: LogEventLevel.Error, rollingInterval: RollingInterval.Day)
    .WriteTo.File("Logs/CriticalLogs.txt", restrictedToMinimumLevel: LogEventLevel.Fatal, rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Replace the default logging with Serilog
builder.Host.UseSerilog();

// Add services to the container.
var conn = builder.Configuration.GetConnectionString("SchoolManagementDbConnection");
builder.Services.AddDbContext<SchoolManagementDbContext>(options => options.UseSqlServer(conn));

// Configure Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;

    // Configure lockout settings to enable rate limiting for login attempts
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(60); // Lockout for 60 minutes
    options.Lockout.MaxFailedAccessAttempts = 5; // Lock out user after 5 failed attempts
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<SchoolManagementDbContext>()
.AddDefaultTokenProviders();

// Configure Password Hasher Options globally
builder.Services.Configure<PasswordHasherOptions>(options =>
{
    // Set iteration count for hashing (higher value increases computational cost)
    options.IterationCount = 100000; // Default is 10,000;
    // Set compatibility mode for hashing
    options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
});

// Configure Cookie-based Authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied"; 
    options.SlidingExpiration = true;
});

// Add session services
builder.Services.AddDistributedMemoryCache(); // Required for session storage
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true; // Make the session cookie HTTP-only
    options.Cookie.IsEssential = true; // Mark the session cookie as essential
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Require HTTPS for session cookies
    options.Cookie.SameSite = SameSiteMode.Strict; // Protect against CSRF
});

// Add rate limiting
builder.Services.AddRateLimiter(config =>
{
    config.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10, // Limit to 10 requests
                Window = TimeSpan.FromMinutes(1), // Per minute
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 5 // Queue up to 5 extra requests
            }));
    config.RejectionStatusCode = 429; // Too Many Requests
});

builder.Services.AddControllersWithViews();

// Configure Toast Notifications
builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 5;
    config.IsDismissable = true;
    config.Position = NotyfPosition.TopRight;
});

// Enforce HTTPS Redirection
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
    options.HttpsPort = 7167; // Ensure this matches HTTPS port
});

var app = builder.Build();

// Seed roles and admin user after building the app
await SeedRoles(app.Services);
await SeedAdminAccount(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Enable HTTPS Redirection Middleware
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Enable session middleware
app.UseSession();

// Add rate limiting middleware
app.UseRateLimiter();

// Add the Content Security Policy (CSP) header
app.Use(async (context, next) =>
{
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; " +
                                                          "img-src 'self' data: https://img.freepik.com https://images.squarespace-cdn.com; " +
                                                          "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdnjs.cloudflare.com https://cdn.jsdelivr.net https://code.jquery.com https://cdn.datatables.net; " +
                                                          "style-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com https://cdn.jsdelivr.net https://cdn.datatables.net https://stackpath.bootstrapcdn.com; " +
                                                          "font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com https://stackpath.bootstrapcdn.com; " +
                                                          "connect-src 'self';";
    await next();
});

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseNotyf();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

/// <summary>
/// Seeds roles (Admin, Teacher, Student) in the application.
/// </summary>
async Task SeedRoles(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roleNames = { "Admin", "Teacher", "Student" };

    foreach (var roleName in roleNames)
    {
        var roleExists = await roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

/// <summary>
/// Seeds an admin account in the application. (Only for testing purposes, remove for production)
/// </summary>
async Task SeedAdminAccount(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    string adminEmail = "admin@example.com";
    string adminPassword = "Edusync123!"; // Replace with a strong password for production
    string adminRole = "Admin";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var newAdmin = new IdentityUser
        {
            UserName = "Admin",
            Email = adminEmail,
            EmailConfirmed = true // Mark email as confirmed for testing
        };

        var result = await userManager.CreateAsync(newAdmin, adminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newAdmin, adminRole);
            Console.WriteLine($"Admin account ({adminEmail}) created successfully.");
        }
        else
        {
            Console.WriteLine($"Failed to create admin account ({adminEmail}):");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"- {error.Description}");
            }
        }
    }
    else
    {
        Console.WriteLine($"Admin account ({adminEmail}) already exists.");
    }
}
