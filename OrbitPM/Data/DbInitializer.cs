using Microsoft.AspNetCore.Identity;
using OrbitPM.Models;
using System.Linq;

namespace OrbitPM.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Check if there is already a Module Leader
            if (context.Users.Any(u => u.Role == "ModuleLeader"))
            {
                return; // DB has been seeded
            }

            var admin = new ApplicationUser
            {
                FullName = "Module Leader",
                Email = "admin@orbitpm.com",
                Role = "ModuleLeader",
                PasswordHash = string.Empty
            };

            var hasher = new PasswordHasher<ApplicationUser>();
            admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");

            context.Users.Add(admin);
            context.SaveChanges();
        }
    }
}
