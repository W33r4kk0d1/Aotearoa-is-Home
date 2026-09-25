using Aotearoa_is_Home.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aotearoa_is_Home.Models.ViewModels;
using Aotearoa_is_Home.Data;

namespace Aotearoa_is_Home.Areas.Users.Controllers
{
    [Area("Users")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        private async Task LoadLanguagesAsync(int? selectedLanguageId)
        {
            ViewBag.Languages = await _context.Languages
                .OrderBy(l => l.Name)
                .ToListAsync();

            ViewBag.SelectedLanguageId = selectedLanguageId;
        }

        // GET: /Users/Users
        public async Task<IActionResult> Index(
            string? search,
            string? role,
            int page = 1)
        {
            const int pageSize = 10;

            // Make sure page is never less than 1
            if (page < 1)
            {
                page = 1;
            }

            // Get all users
            var usersQuery = _userManager.Users
                            .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                usersQuery = usersQuery.Where(u =>
                    u.FirstName.Contains(search) ||
                    u.LastName.Contains(search) ||
                    u.Email!.Contains(search) ||
                    u.UserName!.Contains(search));
            }

            // Get users before pagination
            var users = await usersQuery
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();

            // ROLE FILTER
            if (!string.IsNullOrWhiteSpace(role))
            {
                var filteredUsers = new List<ApplicationUser>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Contains(role))
                    {
                        filteredUsers.Add(user);
                    }
                }

                users = filteredUsers;
            }

            // Total number of users
            var totalUsers = users.Count;

            // Total pages
            var totalPages = (int)Math.Ceiling(
                totalUsers / (double)pageSize);

            // Make sure requested page exists
            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            // Get only the users for this page
            var pagedUsers = users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Get roles for displayed users
            var userRoles = new Dictionary<string, string>();

            foreach (var user in pagedUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userRoles[user.Id] = roles.Count > 0
                    ? string.Join(", ", roles)
                    : "No Role";
            }

            // Send values to the View
            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.UserRoles = userRoles;

            return View(pagedUsers);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.Users
                .Include(u => u.Language)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            ViewBag.UserRoles = roles.Count > 0
                ? string.Join(", ", roles)
                : "No Role";

            return View(user);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                ContactNumber = user.ContactNumber,
                LanguageId = user.LanguageId,
                LinkedInProfile = user.LinkedInProfile,
                Role = roles.FirstOrDefault() ?? string.Empty
            };

            await LoadLanguagesAsync(model.LanguageId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadLanguagesAsync(model.LanguageId);
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                return NotFound();
            }

            var existingEmailUser = await _userManager.FindByEmailAsync(model.Email);

            if (existingEmailUser != null &&
                existingEmailUser.Id != user.Id)
            {
                ModelState.AddModelError(
                    "Email",
                    "This email address is already being used by another user.");

                await LoadLanguagesAsync(model.LanguageId);
                return View(model);
            }

            user.FirstName = model.FirstName.Trim();
            user.LastName = model.LastName.Trim();
            user.Email = model.Email.Trim();
            user.UserName = model.Email.Trim();
            user.NormalizedEmail = _userManager.NormalizeEmail(model.Email.Trim());
            user.NormalizedUserName = _userManager.NormalizeName(model.Email.Trim());
            user.ContactNumber = string.IsNullOrWhiteSpace(model.ContactNumber)
                ? null
                : model.ContactNumber.Trim();
            user.LanguageId = model.LanguageId;
            user.LinkedInProfile = string.IsNullOrWhiteSpace(model.LinkedInProfile)
                ? null
                : model.LinkedInProfile.Trim();

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                await LoadLanguagesAsync(model.LanguageId);
                return View(model);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (!string.IsNullOrWhiteSpace(model.Role))
            {
                if (!currentRoles.Contains(model.Role))
                {
                    if (currentRoles.Count > 0)
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    }

                    await _userManager.AddToRoleAsync(user, model.Role);
                }
            }

            TempData["UserSuccess"] =
                $"User {user.FirstName} {user.LastName} was updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            ViewBag.UserRoles = roles.Count > 0
                ? string.Join(", ", roles)
                : "No Role";

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var userName = $"{user.FirstName} {user.LastName}";

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                var roles = await _userManager.GetRolesAsync(user);

                ViewBag.UserRoles = roles.Count > 0
                    ? string.Join(", ", roles)
                    : "No Role";

                return View("Delete", user);
            }

            TempData["UserSuccess"] =
                $"User {userName} was deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}