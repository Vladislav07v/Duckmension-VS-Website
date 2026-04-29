using Duckmension_Website.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Duckmension_Website.Controllers;

/// <summary>
/// Administrative controller for managing users and their roles.
/// Restricted to users with Admin or Owner roles only.
/// </summary>
[Authorize(Roles = "Admin,Owner")]
public class AdminController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    /// <summary>
    /// Initializes a new instance of the AdminController with required dependency services.
    /// </summary>
    public AdminController(UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>
    /// Displays a list of all users in the system with their current roles.
    /// Provides an overview for administrators to manage user permissions.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        var model = new List<UserRoleRowViewModel>();

        // Build model with each user's information and their primary role
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            model.Add(new UserRoleRowViewModel
            {
                UserId = u.Id,
                Email = u.Email ?? u.UserName ?? "(no email)",
                Role = roles.FirstOrDefault() // Display primary role (assuming one main role per user)
            });
        }

        return View(model);
    }

    /// <summary>
    /// Displays the edit form for changing a user's role assignment.
    /// GET request to load the user and available roles.
    /// </summary>
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var userRoles = await _userManager.GetRolesAsync(user);
        var allRoles = _roleManager.Roles.Select(r => r.Name!).ToList();

        var model = new EditUserRoleViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? user.UserName ?? "(no email)",
            SelectedRole = userRoles.FirstOrDefault() ?? "User",
            Roles = allRoles
        };

        return View(model);
    }

    /// <summary>
    /// Processes the role assignment change for a user.
    /// POST request to update the user's primary role in the system.
    /// Includes security checks to prevent unauthorized role changes.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserRoleViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null) return NotFound();

        // Security check: Prevent users from removing their own Owner or Admin role
        var currentUserId = _userManager.GetUserId(User);
        if (currentUserId == user.Id && model.SelectedRole != "Owner" || model.SelectedRole != "Admin")
        {
            ModelState.AddModelError("", "You are don't have authorization to change this role!");
        }

        if (!ModelState.IsValid)
        {
            model.Roles = _roleManager.Roles.Select(r => r.Name!).ToList();
            return View(model);
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        // Remove all existing roles and assign the new primary role
        if (currentRoles.Any())
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        await _userManager.AddToRoleAsync(user, model.SelectedRole);

        return RedirectToAction(nameof(Index));
    }
}
