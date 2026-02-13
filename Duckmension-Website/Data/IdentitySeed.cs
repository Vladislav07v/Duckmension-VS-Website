using Microsoft.AspNetCore.Identity;

namespace Duckmension_Website.Data
{
    public class IdentitySeed
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var roleManager =
                scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager =
                scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            string[] roles = { "Owner", "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            await EnsureUserWithRole(userManager, "owner@email.local", "Owner123!", "Owner");
            await EnsureUserWithRole(userManager, "admin@email.local", "Admin123!", "Admin");
            await EnsureUserWithRole(userManager, "user@email.local", "User123!", "User");
        }

            private static async Task EnsureUserWithRole(
                UserManager<IdentityUser> userManager,
                string email,
                string password,
                string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Cannot create user {email}: {errors}");
                }
            }
            if (!await userManager.IsInRoleAsync(user, role))
                await userManager.AddToRoleAsync(user, role);
        }
    }
}
