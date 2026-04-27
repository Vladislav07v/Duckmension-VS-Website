using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

        /// <summary>
        /// Ensure existing users have a separate username set. If a user's UserName is empty or equals their email,
        /// this will generate a username from the email local-part and make it unique if necessary.
        /// This method is safe to run multiple times.
        /// </summary>
        public static async Task MigrateUsernamesAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var logger = scope.ServiceProvider.GetService<ILogger<IdentitySeed>>();

            var users = await userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(user.UserName) || string.Equals(user.UserName, user.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        var baseName = (user.Email ?? "user").Split('@')[0];
                        // sanitize
                        baseName = new string(baseName.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-').ToArray()).ToLowerInvariant();
                        if (string.IsNullOrWhiteSpace(baseName)) baseName = "user";

                        var candidate = baseName;
                        var i = 1;
                        while (await userManager.FindByNameAsync(candidate) != null)
                        {
                            candidate = baseName + i.ToString();
                            i++;
                        }

                        var setResult = await userManager.SetUserNameAsync(user, candidate);
                        if (!setResult.Succeeded)
                        {
                            logger?.LogWarning("Failed to set username for user {UserId}: {Errors}", user.Id, string.Join(';', setResult.Errors.Select(e => e.Description)));
                        }
                        else
                        {
                            logger?.LogInformation("Set username for user {UserId} -> {UserName}", user.Id, candidate);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Error migrating username for user {UserId}", user.Id);
                }
            }
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
