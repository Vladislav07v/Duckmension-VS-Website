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
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> UserFind()
        {
            // Load user profiles including the linked Identity user and project to a strongly-typed model
            var profiles = await _db.UserProfiles
                .Include(p => p.User)
                .ToListAsync();

            var model = profiles.Select(p => new UserProfileListItemViewModel
            {
                UserName = p.User?.UserName ?? p.UserId,
                Cookies = p.CookieCount,
                Hats = p.CurrentlyWornHat
            }).ToList();

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
