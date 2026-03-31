using IdentityService.API.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.API.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IdentityDbContext context)
        {
            await SeedRolesAsync(context);
            await SeedAdminUserAsync(context);
        }

        private static async Task SeedRolesAsync(IdentityDbContext context)
        {
            if (!await context.Roles.AnyAsync(r => r.Name == "Admin"))
            {
                context.Roles.Add(new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin"
                });
            }

            if (!await context.Roles.AnyAsync(r => r.Name == "Customer"))
            {
                context.Roles.Add(new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Customer"
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedAdminUserAsync(IdentityDbContext context)
        {
            var existingAdmin = await context.Users
                .FirstOrDefaultAsync(u => u.Email == "admin@local.test");

            if (existingAdmin != null)
                return;

            var adminRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Admin");

            if (adminRole == null)
                throw new Exception("Admin role was not found during seeding.");

            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "System",
                LastName = "Admin",
                Email = "admin@local.test",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(adminUser);

            context.UserRoles.Add(new UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id
            });

            await context.SaveChangesAsync();
        }
    }
}