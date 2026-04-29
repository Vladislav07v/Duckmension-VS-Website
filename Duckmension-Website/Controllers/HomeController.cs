using Duckmension_Website.Models;
using Duckmension_Website.Models.ViewModels;
using Duckmension_Website.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Duckmension_Website.Controllers
{
    /// <summary>
    /// Handles public-facing pages and general application actions.
    /// Manages the homepage, user discovery, and error handling.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// Initializes a new instance of the HomeController with required dependencies.
        /// </summary>
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        /// <summary>
        /// Displays the main landing/home page of the application.
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Displays the updates page.
        /// </summary>
        public IActionResult Update()
        {
            return View();
        }

        /// <summary>
        /// Displays a list of all user profiles with their stats (cookie count, current hat).
        /// Allows users to discover and learn about other players.
        /// </summary>
        public async Task<IActionResult> UserFind()
        {
            // Load user profiles including the linked Identity user and project to a strongly-typed model
            var profiles = await _db.UserProfiles
                .Include(p => p.User)
                .ToListAsync();

            // Map to view model with sanitized user display data
            var model = profiles.Select(p => new UserProfileListItemViewModel
            {
                UserName = p.User?.UserName ?? p.UserId,
                Cookies = p.CookieCount,
                Hats = p.CurrentlyWornHat
            }).ToList();

            return View(model);
        }

        /// <summary>
        /// Displays error details when an unhandled exception occurs.
        /// Response caching is disabled to ensure fresh error information each time.
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
