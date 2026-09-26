using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models;
using Aotearoa_is_Home.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Super Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Total registered accounts
            var totalAccounts = await _userManager.Users.CountAsync();

            // Pending student verifications
            var pendingVerifications =
                await _context.PendingStudentRegistrations
                    .CountAsync(x => x.Status == "Pending");

            // Service Provider accounts
            var serviceProviderRole =
                await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == "Service Provider");

            var serviceProviderCount = 0;

            if (serviceProviderRole != null)
            {
                serviceProviderCount =
                    await _context.UserRoles
                        .CountAsync(ur => ur.RoleId == serviceProviderRole.Id);
            }

            // Start of the current week (Monday)
            var today = DateTime.UtcNow.Date;

            var daysSinceMonday =
                ((int)today.DayOfWeek + 6) % 7;

            var startOfWeek =
                today.AddDays(-daysSinceMonday);

            // New accounts this week
            var newThisWeek =
                await _userManager.Users
                    .CountAsync(u =>
                        u.CreatedAt != null &&
                        u.CreatedAt >= startOfWeek);

            // Recent users
            var recentUsers =
                await _userManager.Users
                    .Where(u => u.CreatedAt != null)
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .Select(u => new RecentUserViewModel
                    {
                        FullName = u.FirstName + " " + u.LastName,

                        Email = u.Email ?? string.Empty,

                        CreatedAt = u.CreatedAt
                    })
                    .ToListAsync();

            // Get roles for recent users
            foreach (var recentUser in recentUsers)
            {
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u =>
                        u.Email == recentUser.Email);

                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    recentUser.Role = roles.FirstOrDefault() ?? "No Role";
                }
            }

            var model = new AdminDashboardViewModel
            {
                TotalAccounts = totalAccounts,
                PendingVerifications = pendingVerifications,
                ServiceProviders = serviceProviderCount,
                NewThisWeek = newThisWeek,
                RecentUsers = recentUsers
            };

            return View(model);
        }
    }
}