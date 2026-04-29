using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Duckmension_Website.Data
{
    /// <summary>
    /// Handles seeding of initial identity data (roles and default users) and username migration for existing users.
    /// </summary>
    public class IdentitySeed
    {
        /// <summary>
        /// Seeds the database with initial roles (Owner, Admin, User) and default test users for each role.
        /// This is called during application startup to ensure required roles and users exist.
        /// </summary>
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var roleManager =
                scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager =
                scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Define the application roles: Owner manages the app, Admin manages content, User is a regular player
            string[] roles = { "Owner", "Admin", "User" };

            // Create roles if they don't already exist
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Create or ensure default test users exist with their respective roles
            await EnsureUserWithRole(userManager, "owner@email.local", "Owner123!", "Owner");
            await EnsureUserWithRole(userManager, "admin@email.local", "Admin123!", "Admin");
            await EnsureUserWithRole(userManager, "user@email.local", "User123!", "User");
        }

        /// <summary>
        /// Ensures all existing users have a unique username separate from their email.
        /// If a user's UserName is empty or matches their email, generates a sanitized username from the email.
        /// This method is idempotent and can be safely run multiple times.
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
                    // Check if user needs a username (empty or same as email)
                    if (string.IsNullOrWhiteSpace(user.UserName) || string.Equals(user.UserName, user.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        // Extract the local part of the email (before @) as the base username
                        var baseName = (user.Email ?? "user").Split('@')[0];

                        // Remove invalid characters from the username (only allow letters, digits, underscore, hyphen)
                        baseName = new string(baseName.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-').ToArray()).ToLowerInvariant();
                        if (string.IsNullOrWhiteSpace(baseName)) baseName = "user";

                        // Ensure the username is unique by appending a number if necessary
                        var candidate = baseName;
                        var i = 1;
                        while (await userManager.FindByNameAsync(candidate) != null)
                        {
                            candidate = baseName + i.ToString();
                            i++;
                        }

                        // Update the user with the new username
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

        /// <summary>
        /// Helper method to ensure a user exists with the specified email, password, and role.
        /// Creates the user if it doesn't exist, and assigns the role if not already assigned.
        /// </summary>
        /// <param name="userManager">The UserManager service for managing users</param>
        /// <param name="email">The email address of the user</param>
        /// <param name="password">The password for the new user</param>
        /// <param name="role">The role to assign to the user</param>
        private static async Task EnsureUserWithRole(
                UserManager<IdentityUser> userManager,
                string email,
                string password,
                string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Create new user if doesn't exist
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

            // Assign role to user if not already assigned
            if (!await userManager.IsInRoleAsync(user, role))
                await userManager.AddToRoleAsync(user, role);
        }
    }
}
