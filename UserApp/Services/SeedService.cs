using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UserApp.Models;

namespace UserApp.Services
{
    public class SeedService
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

            try
            {
                logger.LogInformation("===== Identity Seeding Started =====");

                // 1️⃣ Seed Roles
                await CreateRoleIfNotExists(roleManager, "Admin");
                await CreateRoleIfNotExists(roleManager, "User");

                // 2️⃣ Seed Admin User
                await CreateUserIfNotExists(
                    userManager,
                    logger,
                    email: "admin@123.com",
                    password: "Admin@123",
                    role: "Admin",
                    fullName: "Administrator"
                );

                // 3️⃣ Seed Normal User
                await CreateUserIfNotExists(
                    userManager,
                    logger,
                    email: "user@123.com",
                    password: "User@123",
                    role: "User",
                    fullName: "Normal User"
                );

                logger.LogInformation("===== Identity Seeding Completed =====");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during Identity seeding");
            }
        }

        // ----------------- Helper Methods -----------------

        private static async Task CreateRoleIfNotExists(
            RoleManager<IdentityRole> roleManager,
            string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        private static async Task CreateUserIfNotExists(
            UserManager<Users> userManager,
            ILogger logger,
            string email,
            string password,
            string role,
            string fullName)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user != null)
            {
                // Ensure role exists for existing user
                if (!await userManager.IsInRoleAsync(user, role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }
                return;
            }

            var newUser = new Users
            {
                UserName = email,          // IMPORTANT
                Email = email,
                EmailConfirmed = true,     // IMPORTANT
                FullName = fullName
            };

            var result = await userManager.CreateAsync(newUser, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newUser, role);
                logger.LogInformation($"User '{email}' created with role '{role}'");
            }
            else
            {
                logger.LogWarning(
                    $"Failed to create user '{email}': " +
                    string.Join(", ", result.Errors.Select(e => e.Description))
                );
            }
        }
    }
}
