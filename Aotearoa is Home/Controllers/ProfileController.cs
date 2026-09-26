using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models;
using Aotearoa_is_Home.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Aotearoa_is_Home.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var user = await _userManager.Users
                .Include(u => u.Language)
                .FirstOrDefaultAsync(u => u.Id == userId);

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

        // GET: /Profile/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                ContactNumber = user.ContactNumber,
                LanguageId = user.LanguageId,
                LinkedInProfile = user.LinkedInProfile
            };

            await LoadLanguagesAsync(model.LanguageId);

            return View(model);
        }

        // POST: /Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadLanguagesAsync(model.LanguageId);
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var existingEmailUser =
                await _userManager.FindByEmailAsync(model.Email);

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

            user.NormalizedEmail =
                _userManager.NormalizeEmail(model.Email.Trim());

            user.NormalizedUserName =
                _userManager.NormalizeName(model.Email.Trim());

            user.ContactNumber =
                string.IsNullOrWhiteSpace(model.ContactNumber)
                    ? null
                    : model.ContactNumber.Trim();

            user.LanguageId = model.LanguageId;

            user.LinkedInProfile =
                string.IsNullOrWhiteSpace(model.LinkedInProfile)
                    ? null
                    : model.LinkedInProfile.Trim();

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                await LoadLanguagesAsync(model.LanguageId);

                return View(model);
            }

            TempData["ProfileSuccess"] =
                "Your profile was updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadLanguagesAsync(int? selectedLanguageId)
        {
            ViewBag.Languages = await _context.Languages
                .OrderBy(l => l.Name)
                .ToListAsync();

            ViewBag.SelectedLanguageId = selectedLanguageId;
        }
    }
}