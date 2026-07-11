using backend.Entities;
using backend.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(ApplicationDbContext context)
        {
            const string adminEmail = "admin@app.com";

            // check if an admin already exists — don't create duplicates
            var adminExists = await context.Users
                .AnyAsync(u => u.Role == UserRole.Admin);

            if (adminExists)
                return;

            var admin = new User
            {
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
                Role = UserRole.Admin
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}
