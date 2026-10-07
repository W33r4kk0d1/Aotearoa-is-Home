using Aotearoa_is_Home.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
            }// Else ends

            // ==========================================
            // CREATE 2026 CALENDAR INFORMATION
            // ==========================================

            var context =
                serviceProvider.GetRequiredService<ApplicationDbContext>();

            var calendarInformation = new List<CalendarInformation>
            {
                // ------------------------------------------
                // NATIONAL PUBLIC HOLIDAYS
                // ------------------------------------------

                new CalendarInformation
                {
                    Title = "New Year's Day",
                    Description = "New Year's Day is a public holiday in New Zealand. Many businesses may operate reduced or different hours. Check individual store opening hours before travelling.",
                    Date = new DateTime(2026, 1, 1),
                    Type = "Public Holiday",
                    IsPublicHoliday = true,
                    IsShopClosure = false,
                    Region = null
                },

                new CalendarInformation
                {
                    Title = "Day after New Year's Day",
                    Description = "Public holiday in New Zealand. Many businesses may operate reduced or different hours. Plan essential shopping in advance and check individual store opening hours.",
                    Date = new DateTime(2026, 1, 2),
                    Type = "Public Holiday",
                    IsPublicHoliday = true,
                    IsShopClosure = false,
                    Region = null
                },

                new CalendarInformation
                {
                    Title = "Waitangi Day",
                    Description = "Waitangi Day is a New Zealand public holiday. Check individual business opening hours.",
                    Date = new DateTime(2026, 2, 6),
                    Type = "Public Holiday",
                    IsPublicHoliday = true,
                    IsShopClosure = false,
                    Region = null
                },

                new CalendarInformation
                {
                    Title = "Good Friday",
                    Description = "Good Friday is a public holiday. Most shops must close under New Zealand shop-trading restrictions.",
                    Date = new DateTime(2026, 4, 3),
                    Type = "Public Holiday",
                    IsPublicHoliday = true,
                    IsShopClosure = true,
                    Region = null
                },

                new CalendarInformation
                {
                    Title = "Easter Monday",
                    Description = "Easter Monday is a New Zealand public holiday. Check individual business opening hours.",
                    Date = new DateTime(2026, 4, 6),
                    Type = "Public Holiday",
                    IsPublicHoliday = true,
                    IsShopClosure = false,
                    Region = null
                },

                new CalendarInformation
                {
                    Title = "Anzac Day",
                    Description = "Anzac Day is a public holiday. Most shops must remain closed until 1:00 PM.",
                    Date = new DateTime(2026, 4, 25),
                    Type = "Public Holiday",
                    IsPublicHoliday = true,
                    IsShopClosure = true,
                    Region = null
                },

                new CalendarInformation
                {
                    Title = "Matariki",
                    Description = "Matariki is a New Zealand public holiday. Check individual business opening hours.",
                    Date = new DateTime(2026, 7, 10),
                    Type = "Public Holiday",
                    IsPublicHoliday = true,
                    IsShopClosure = false,
                    Region = null
                },

                new CalendarInformation
                {
                    Title = "Labour Day",
                    Description = "Labour Day is a New Zealand public holiday. Check individual business opening hours.",
                    Date = new DateTime(2026, 10, 26),
                    Type = "Public Holiday",
                    IsPublicHoliday = true,
                    IsShopClosure = false,
                    Region = null
                },

                new CalendarInformation
                {
                    Title = "Christmas Day",
                    Description = "Christmas Day is a public holiday. Most shops must close. Plan grocery shopping and essential purchases in advance.",
                    Date = new DateTime(2026, 12, 25),
                    Type = "Public Holiday",
                    IsPublicHoliday = true,
                    IsShopClosure = true,
                    Region = null
                },

                new CalendarInformation
                {
                    Title = "Boxing Day",
                    Description = "Boxing Day is a public holiday. Many businesses may have different or reduced opening hours. Check individual store opening hours before travelling.",
                    Date = new DateTime(2026, 12, 26),
                    Type = "Public Holiday",
                    IsPublicHoliday = true,
                    IsShopClosure = false,
                    Region = null
                },

                // ------------------------------------------
                // WELLINGTON REGIONAL ANNIVERSARY DAY
                // ------------------------------------------

                new CalendarInformation
                {
                    Title = "Wellington Anniversary Day",
                    Description = "Wellington Anniversary Day is a regional public holiday for the Wellington region. Check individual business opening hours.",
                    Date = new DateTime(2026, 1, 19),
                    Type = "Regional Holiday",
                    IsPublicHoliday = true,
                    IsShopClosure = false,
                    Region = "Wellington"
                },

                // ------------------------------------------
                // IMPORTANT SHOPPING REMINDERS
                // ------------------------------------------

                new CalendarInformation
                {
                    Title = "Christmas Shopping Reminder",
                    Description = "Plan grocery shopping and essential purchases before Christmas Day because most shops must close on Christmas Day.",
                    Date = new DateTime(2026, 12, 24),
                    Type = "Reminder",
                    IsPublicHoliday = false,
                    IsShopClosure = false,
                    Region = null
                },

                new CalendarInformation
                {
                    Title = "New Year Shopping Reminder",
                    Description = "Plan essential grocery shopping around New Year's Day and the following public holiday. Many businesses may operate reduced or different hours.",
                    Date = new DateTime(2026, 12, 31),
                    Type = "Reminder",
                    IsPublicHoliday = false,
                    IsShopClosure = false,
                    Region = null
                }
            };

            // Add only records that do not already exist.
            foreach (var information in calendarInformation)
            {
                var alreadyExists = await context.CalendarInformations
                    .AnyAsync(x =>
                        x.Title == information.Title &&
                        x.Date == information.Date);

                if (!alreadyExists)
                {
                    context.CalendarInformations.Add(information);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}