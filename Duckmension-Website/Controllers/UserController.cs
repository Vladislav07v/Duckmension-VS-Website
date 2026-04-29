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

    /// <summary>
    /// Initializes a new instance of the UserController with required dependency services.
    /// </summary>
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

        // Retrieve user profile or create default profile for new users
        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
        {
            // Create a default profile for new users to avoid null reference in the view
            profile = new UserProfile
            {
                UserId = userId,
                CookieCount = 0,
                CurrentlyWornHat = 0,
                OwnedHats = new List<int>()
            };
            _db.UserProfiles.Add(profile);
            await _db.SaveChangesAsync();
        }

        // Ensure the Identity user is loaded so views can display the username
        var identityUser = await _userManager.FindByIdAsync(userId);
        profile.User = identityUser;

        return View(profile);
    }

    /// <summary>
    /// POST: Updates the user's profile information.
    /// Validates that the user can only update their own profile.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(UserProfile model)
    {
        var userId = _userManager.GetUserId(User)!;
        // Security: Ensure user can only update their own profile
        if (model.UserId != userId) return Forbid();

        if (!ModelState.IsValid) return View(model);

        _db.UserProfiles.Update(model);
        await _db.SaveChangesAsync();

        TempData["msg"] = "The profile has been saved.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// POST: Updates the user's display name (username).
    /// Validates that the new display name is not already taken by another user.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(string UserId, string DisplayName)
    {
        var currentUserId = _userManager.GetUserId(User)!;
        // Security: Ensure user can only update their own display name
        if (UserId != currentUserId) return Forbid();

        // Validate that display name is not empty
        if (string.IsNullOrWhiteSpace(DisplayName))
        {
            TempData["error"] = "Display name cannot be empty.";
            TempData["openTab"] = "settings";
            return RedirectToAction(nameof(Index));
        }

        // Check if another user already has this display name
        var existing = await _userManager.FindByNameAsync(DisplayName);
        if (existing != null && existing.Id != currentUserId)
        {
            TempData["error"] = "That display name is already taken. Please choose another.";
            TempData["openTab"] = "settings";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user == null) return NotFound();

        // Update the username in Identity
        user.UserName = DisplayName;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            TempData["error"] = string.Join(" ", updateResult.Errors.Select(e => e.Description));
            TempData["openTab"] = "settings";
            return RedirectToAction(nameof(Index));
        }

        // Ensure profile exists and is updated
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
    /// Validates the old password and ensures the new password is confirmed.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string UserId, string OldPassword, string NewPassword, string ConfirmPassword)
    {
        var currentUserId = _userManager.GetUserId(User)!;
        // Security: Ensure user can only change their own password
        if (UserId != currentUserId) return Forbid();

        // Validate that both old and new passwords are provided
        if (string.IsNullOrWhiteSpace(OldPassword) || string.IsNullOrWhiteSpace(NewPassword))
        {
            TempData["error"] = "Please provide both current and new passwords.";
            TempData["openTab"] = "settings";
            return RedirectToAction(nameof(Index));
        }

        // Validate that new password and confirmation match
        if (NewPassword != ConfirmPassword)
        {
            TempData["error"] = "New password and confirmation do not match.";
            TempData["openTab"] = "settings";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user == null) return NotFound();

        // Attempt to change the password
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
