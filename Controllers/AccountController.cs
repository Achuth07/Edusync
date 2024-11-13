using Edusync.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Edusync.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            _logger.LogInformation("Navigated to Register page.");
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            // Server-side validation to ensure only expected data is processed
            if (ModelState.IsValid)
            {
                // Create a new user
                var user = new IdentityUser { UserName = model.Username, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Username} registered successfully.", model.Username);

                    // Add the user to the specified role
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        // Ensure the role is valid before adding to avoid unexpected input
                        var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
                        if (roleResult.Succeeded)
                        {
                            _logger.LogInformation("User {Username} assigned role {Role}.", model.Username, model.Role);
                        }
                        else
                        {
                            foreach (var error in roleResult.Errors)
                            {
                                _logger.LogWarning("Failed to assign role {Role} to user {Username}: {Error}", model.Role, model.Username, error.Description);
                                ModelState.AddModelError(string.Empty, $"Failed to assign role: {error.Description}");
                            }
                        }
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                // Log any issues that occurred during registration
                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("Failed to register user {Username}: {Error}", model.Username, error.Description);
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                _logger.LogWarning("Registration form validation failed for user {Username}.", model.Username);
            }

            // Re-render the form with validation errors if any
            return View(model);
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            _logger.LogInformation("Navigated to Login page.");
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            // Server-side validation to ensure only expected data is processed
            if (ModelState.IsValid)
            {
                // Check if the user exists
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user == null)
                {
                    _logger.LogWarning("Login attempt failed: User {Username} not found.", model.Username);
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }

                // Check if the user is locked out
                if (await _userManager.IsLockedOutAsync(user))
                {
                    _logger.LogWarning("Login attempt for locked-out user {Username}.", model.Username);
                    ModelState.AddModelError(string.Empty, "Your account is locked out. Please try again later.");
                    return View(model);
                }

                // Attempt to sign in the user
                var result = await _signInManager.PasswordSignInAsync(
                    model.Username,
                    model.Password,
                    isPersistent: false,
                    lockoutOnFailure: true // Enables lockout for failed attempts
                );

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Username} logged in successfully.", model.Username);

                    // Reset failed access count upon successful login
                    await _userManager.ResetAccessFailedCountAsync(user);

                    return RedirectToAction("Index", "Home");
                }
                else if (result.IsLockedOut)
                {
                    _logger.LogWarning("User {Username} is locked out after failed login attempts.", model.Username);
                    ModelState.AddModelError(string.Empty, "Your account is locked out. Please try again later after 60 mins.");
                }
                else
                {
                    // Increment failed access count
                    await _userManager.AccessFailedAsync(user);
                    _logger.LogWarning("Failed login attempt for user {Username}.", model.Username);
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }
            else
            {
                _logger.LogWarning("Login form validation failed for user {Username}.", model.Username);
            }

            // Re-render the form with validation errors if any
            return View(model);
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Clear the session
            HttpContext.Session.Clear();
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out successfully.");
            return RedirectToAction("Index", "Home");
        }
    }
}
