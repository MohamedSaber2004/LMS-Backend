using LMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LMS.Persistence.Seeding.Base
{
    public static class BaseSeeder
    {

        public static async Task SeedAllAsync(LmsDbContext context, UserManager<ApplicationUser>? userManager = null, RoleManager<IdentityRole<Guid>>? roleManager = null)
        {
            await CountrySeeder.SeedCountriesAsync(context);
            await UserSeeder.SeedUsersAndRolesAsync(userManager, roleManager);
        }
    }
}
