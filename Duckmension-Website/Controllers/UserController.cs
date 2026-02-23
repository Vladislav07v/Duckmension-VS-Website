using Duckmension_Website.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Duckmension_Website.Data;
using Duckmension_Website.Models;

namespace Duckmension_Website.Controllers;

[Authorize(Roles = "User,Owner")]
public class UserController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public UserController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;

        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
        {
            profile = new UserProfile { UserId = userId, CookieCount = 8, CurrentlyWornHat = 1, OwnedHats = {1,2,3,4,5} };
            _db.UserProfiles.Add(profile);
            await _db.SaveChangesAsync();
        }

        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(UserProfile model)
    {
        var userId = _userManager.GetUserId(User)!;
        if (model.UserId != userId) return Forbid();

        if (!ModelState.IsValid) return View(model);

        _db.UserProfiles.Update(model);
        await _db.SaveChangesAsync();

        TempData["msg"] = "The profile has been saved.";
        return RedirectToAction(nameof(Index));
    }
}
