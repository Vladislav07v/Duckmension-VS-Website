using Duckmension_Website.Data;
using Duckmension_Website.Models;
using Duckmension_Website.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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
        /// </summary>
        public async Task<IActionResult> UserFind()
        {
            var profiles = await _db.UserProfiles
                .Include(p => p.User)
                .ToListAsync();

            var model = profiles.Select(p => new UserProfileListItemViewModel
            {
                UserName = p.User?.UserName ?? p.UserId,
                Cookies = p.Cookies,
                Hats = p.CurrentlyWornHat
            }).ToList();

            return View(model);
        }

        /// <summary>
        /// Displays error details when an unhandled exception occurs.
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}