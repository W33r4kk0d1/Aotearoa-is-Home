using Aotearoa_is_Home.Models;
using Microsoft.AspNetCore.Identity;

namespace Aotearoa_is_Home.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager =
                serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles =
            {
                "Student",
                "Admin",
                "Super Admin",
                "Service Provider",
                "Family Member"
            };

            // Create roles if they do not already exist
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }

            // ==========================================
            // CREATE INITIAL SUPER ADMIN
            // ==========================================

            const string superAdminEmail = "superadmin@aotearoaishome.com";
            const string superAdminPassword = "SuperAdmin@12345";

            var existingSuperAdmin =
                await userManager.FindByEmailAsync(superAdminEmail);

            if (existingSuperAdmin == null)
            {
                var superAdmin = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    EmailConfirmed = true,
                    FirstName = "Super",
                    LastName = "Administrator",
                    CreatedAt = DateTime.UtcNow
                };

                var createResult =
                    await userManager.CreateAsync(
                        superAdmin,
                        superAdminPassword);

                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(
                        superAdmin,
                        "Super Admin");
                }
            }
            else
            {
                // Make sure the existing account has Super Admin role
                if (!await userManager.IsInRoleAsync(
                        existingSuperAdmin,
                        "Super Admin"))
                {
                    await userManager.AddToRoleAsync(
                        existingSuperAdmin,
                        "Super Admin");
                }
            }
        }
    }
}