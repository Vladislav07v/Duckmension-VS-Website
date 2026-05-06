using Duckmension_Website.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Duckmension_Website.Models;
using System.Linq;

namespace Duckmension_Website.Controllers;

/// <summary>
/// User profile controller for managing personal user data and settings.
/// Restricted to authenticated users with User or Owner roles.
/// Handles profile viewing, editing, display name changes, and password management.
/// </summary>
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

    /// <summary>
    /// GET: Displays the user's profile page with their game statistics and settings.
    /// Creates a default profile for new users to ensure they have data to display.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;

        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
        {
            profile = new UserProfile
            {
                UserId = userId,
                Cookies = 0,
                CurrentlyWornHat = 0,
                OwnedHats = new List<int>()
            };
            _db.UserProfiles.Add(profile);
            await _db.SaveChangesAsync();
        }

        // Attach the Identity user so the view can show the username
        profile.User = await _userManager.FindByIdAsync(userId);

        return View(profile);
    }

    /// <summary>
    /// POST: Updates the user's profile information.
    /// </summary>
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

    /// <summary>
    /// POST: Updates the user's display name (username).
    /// Validates that the new display name is not already taken.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(string UserId, string DisplayName)
    {
        var currentUserId = _userManager.GetUserId(User)!;
        if (UserId != currentUserId) return Forbid();

        if (string.IsNullOrWhiteSpace(DisplayName))
        {
            TempData["error"] = "Display name cannot be empty.";
            TempData["openTab"] = "settings";
            return RedirectToAction(nameof(Index));
        }

        var existing = await _userManager.FindByNameAsync(DisplayName);
        if (existing != null && existing.Id != currentUserId)
        {
            TempData["error"] = "That display name is already taken. Please choose another.";
            TempData["openTab"] = "settings";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user == null) return NotFound();

        user.UserName = DisplayName;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            TempData["error"] = string.Join(" ", updateResult.Errors.Select(e => e.Description));
            TempData["openTab"] = "settings";
            return RedirectToAction(nameof(Index));
        }

        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == currentUserId);
        if (profile != null)
        {
            _db.UserProfiles.Update(profile);
            await _db.SaveChangesAsync();
        }

        TempData["msg"] = "Display name updated.";
        TempData["openTab"] = "settings";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// POST: Changes the user's password.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string UserId, string OldPassword, string NewPassword, string ConfirmPassword)
    {
        var currentUserId = _userManager.GetUserId(User)!;
        if (UserId != currentUserId) return Forbid();

        if (string.IsNullOrWhiteSpace(OldPassword) || string.IsNullOrWhiteSpace(NewPassword))
        {
            TempData["error"] = "Please provide both current and new passwords.";
            TempData["openTab"] = "settings";
            return RedirectToAction(nameof(Index));
        }

        if (NewPassword != ConfirmPassword)
        {
            TempData["error"] = "New password and confirmation do not match.";
            TempData["openTab"] = "settings";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user == null) return NotFound();

        var result = await _userManager.ChangePasswordAsync(user, OldPassword, NewPassword);
        if (!result.Succeeded)
        {
            TempData["error"] = string.Join(" ", result.Errors.Select(e => e.Description));
            TempData["openTab"] = "settings";
            return RedirectToAction(nameof(Index));
        }

        TempData["msg"] = "Password changed successfully.";
        TempData["openTab"] = "settings";
        return RedirectToAction(nameof(Index));
    }
}