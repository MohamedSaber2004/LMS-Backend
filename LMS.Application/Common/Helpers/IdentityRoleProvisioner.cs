using Microsoft.AspNetCore.Identity;

namespace LMS.Application.Common.Helpers
{
    public static class IdentityRoleProvisioner
    {
        // The deployed box never runs the seeder, so a role can be missing its AspNetRoles row.
        public static async Task EnsureRoleAsync(RoleManager<IdentityRole<Guid>> roleManager, string roleName)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                return;
            }

            await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        }
    }
}
