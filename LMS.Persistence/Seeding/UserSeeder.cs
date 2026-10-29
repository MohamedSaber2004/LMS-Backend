using LMS.Domain.Entities;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace LMS.Persistence.Seeding
{
    public class UserSeeder
    {
        public static async Task SeedUsersAndRolesAsync(UserManager<ApplicationUser>? userManager, RoleManager<IdentityRole<Guid>>? roleManager)
        {
            if (userManager == null || roleManager == null)
                return;

            string[] roles = { nameof(UserType.Admin), nameof(UserType.Learner), nameof(UserType.Instructor) };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                }
            }

            var adminEmail = "admin@gmail.com";
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    PhoneNumber = "1022812243",
                };
                adminUser.SetFullName("System Administrator");
                adminUser.EmailConfirmed = true;
                adminUser.CreatedAt = DateTime.UtcNow;
                adminUser.CreatedBy = "System";

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to seed admin user: {string.Join("; ", result.Errors.Select(e => e.Description))}");
                }

                await userManager.AddToRoleAsync(adminUser, nameof(UserType.Admin));
            }
            else if (!await userManager.IsInRoleAsync(existingAdmin, nameof(UserType.Admin)))
            {
                await userManager.AddToRoleAsync(existingAdmin, nameof(UserType.Admin));
            }
        }
    }
}
