using Duckmension_Website.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Duckmension_Website.Data
{
    /// <summary>
    /// Entity Framework Core database context for the Duckmension VS application.
    /// Extends IdentityDbContext to include both ASP.NET Core Identity tables
    /// and custom application tables.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext
    {
        /// <summary>
        /// DbSet for user profile data associated with each identity user.
        /// </summary>
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

        /// <summary>
        /// Initializes a new instance of the ApplicationDbContext.
        /// </summary>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Configures the database schema and relationships between entities.
        /// Sets up the one-to-one relationship between UserProfile and IdentityUser
        /// with cascade delete so a profile is removed when its user is deleted.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserProfile>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}