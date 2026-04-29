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
        /// <summary>
        /// Application startup method that configures services and middleware.
        /// Handles database migrations, identity setup, and request pipeline configuration.
        /// </summary>
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // Configure Entity Framework Core with SQL Server database connection
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Configure ASP.NET Core Identity with role support
            // RequireConfirmedAccount is set to false to allow users to log in immediately after registration
            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            }).AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

            // Add MVC controllers and views support
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline based on environment.
            if (app.Environment.IsDevelopment())
            {
                // In development, show detailed error pages with database migration information
                app.UseMigrationsEndPoint();
            }
            else
            {
                // In production, use generic error handler and enable HSTS (HTTP Strict Transport Security)
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Enable HTTPS redirection and static file serving
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Configure routing and authorization middleware
            app.UseRouting();

            app.UseAuthorization();

            // Map controller routes with default route to Home/Index
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Map Razor Pages for Identity areas
            app.MapRazorPages();

            // Run database migrations automatically on startup
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.MigrateAsync();
            }

            // Seed initial data (admin users, roles, etc.)
            await IdentitySeed.SeedAsync(app.Services);
            await Duckmension_Website.Data.IdentitySeed.SeedAsync(app.Services);

            // Migrate existing users to have usernames if needed
            await Duckmension_Website.Data.IdentitySeed.MigrateUsernamesAsync(app.Services);

            app.Run();
        }
    }
}
