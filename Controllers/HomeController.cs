using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Edusync.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Edusync.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            _logger.LogInformation("User {UserId} accessed the Index page.", User?.Identity?.Name);
            return View();
        }

        public IActionResult About()
        {
            _logger.LogInformation("User {UserId} accessed the About page.", User?.Identity?.Name);
            return View();
        }

        public IActionResult Privacy()
        {
            _logger.LogInformation("User {UserId} accessed the Privacy page.", User?.Identity?.Name);
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogError("Error encountered with Request ID: {RequestId} by User {UserId}", requestId, User?.Identity?.Name);

            // Log additional debugging information if needed
            if (HttpContext.Request != null)
            {
                _logger.LogDebug("Request Path: {Path}, Method: {Method}", HttpContext.Request.Path, HttpContext.Request.Method);
            }

            return View(new ErrorViewModel { RequestId = requestId });
        }
    }
}
