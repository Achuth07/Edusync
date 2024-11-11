using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Edusync.Models;
using Microsoft.AspNetCore.Authorization;

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
            _logger.LogInformation("User accessed the Index page.");
            return View();
        }

        public IActionResult About()
        {
            _logger.LogInformation("User accessed the About page.");
            return View();
        }

        public IActionResult Privacy()
        {
            _logger.LogInformation("User accessed the Privacy page.");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogError("Error encountered with Request ID: {RequestId}", requestId);
            return View(new ErrorViewModel { RequestId = requestId });
        }
    }
}
