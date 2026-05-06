using Duckmension_Website.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Duckmension_Website
{
    /// <summary>
    /// Main application entry point and configuration class.
    /// Configures all services, middleware, and database setup for the Duckmension VS website.
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Entity Framework Core with SQL Server
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Configure ASP.NET Core Identity with role support.
            // RequireConfirmedAccount = false so users can log in immediately after registration.
            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

            // Add MVC controllers and views, plus JSON support for the Game API endpoints.
            builder.Services.AddControllersWithViews()
                .AddJsonOptions(options =>
                {
                    // Return camelCase JSON (matches what the Lua parser expects)
                    options.JsonSerializerOptions.PropertyNamingPolicy =
                        System.Text.Json.JsonNamingPolicy.CamelCase;
                });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            // Run database migrations automatically on startup
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.MigrateAsync();
            }

            // Seed initial roles and admin/owner users
            await IdentitySeed.SeedAsync(app.Services);
            await Duckmension_Website.Data.IdentitySeed.SeedAsync(app.Services);

            // Migrate existing users that may be missing a UserName
            await Duckmension_Website.Data.IdentitySeed.MigrateUsernamesAsync(app.Services);

            app.Run();
        }
    }
}