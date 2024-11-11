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
})
.AddEntityFrameworkStores<SchoolManagementDbContext>()
.AddDefaultTokenProviders();

// Configure Cookie-based Authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied"; // Optional: Create an AccessDenied view for unauthorized access
    options.SlidingExpiration = true;
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
    options.HttpsPort = 7167; // Ensure this matches your HTTPS port
});

var app = builder.Build();

// Seed roles after building the app
await SeedRoles(app.Services);

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
