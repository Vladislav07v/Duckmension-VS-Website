using Duckmension_Website.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Duckmension_Website.Controllers;

[Authorize(Roles = "Owner")]
public class OwnerController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public OwnerController(UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // 3.1. Списък 
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        var model = new List<UserRoleRowViewModel>();

        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            model.Add(new UserRoleRowViewModel
            {
                UserId = u.Id,
                Email = u.Email ?? u.UserName ?? "(no email)",
                Role = roles.FirstOrDefault() // приемаме 1 основна роля 
            });
        }

        return View(model);
    }

    // 3.2. Edit (GET) 
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

    // 3.3. Edit (POST) 
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserRoleViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null) return NotFound();

        // По избор: забрана Owner да сваля собствената си роля 
        var currentUserId = _userManager.GetUserId(User);
        if (currentUserId == user.Id && model.SelectedRole != "Owner")
        {
            ModelState.AddModelError("", "You can't remove your own role, silly!");
        }

        if (!ModelState.IsValid)
        {
            model.Roles = _roleManager.Roles.Select(r => r.Name!).ToList();
            return View(model);
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        // Премахваме всички роли и добавяме избраната (една основна роля) 
        if (currentRoles.Any())
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        await _userManager.AddToRoleAsync(user, model.SelectedRole);

        return RedirectToAction(nameof(Index));
    }
}
