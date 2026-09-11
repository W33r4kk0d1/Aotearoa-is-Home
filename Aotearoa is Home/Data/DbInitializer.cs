using Microsoft.AspNetCore.Identity;

namespace Aotearoa_is_Home.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider.GetRequiredService
                <RoleManager<IdentityRole>>();

            string[] roles =
            {
                "Student",
                "Admin",
                "Event Provider",
                "Family Member"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }
        }
    }
}