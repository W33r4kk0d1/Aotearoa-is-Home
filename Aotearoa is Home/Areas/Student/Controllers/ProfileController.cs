using Aotearoa_is_Home.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Aotearoa_is_Home.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string FirstName,
            string LastName,
            string Email,
            string? ContactNumber,
            int? LanguageId,
            string? LinkedInProfile)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(FirstName))
            {
                ModelState.AddModelError("FirstName", "First name is required.");
            }

            if (string.IsNullOrWhiteSpace(LastName))
            {
                ModelState.AddModelError("LastName", "Last name is required.");
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ModelState.AddModelError("Email", "Email is required.");
            }

            if (!ModelState.IsValid)
            {
                var model = new ApplicationUser
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = Email,
                    FirstName = FirstName,
                    LastName = LastName,
                    ContactNumber = ContactNumber,
                    LanguageId = LanguageId,
                    LinkedInProfile = LinkedInProfile,
                    PhoneNumber = user.PhoneNumber,
                    CreatedAt = user.CreatedAt
                };

                return View(model);
            }

            user.FirstName = FirstName;
            user.LastName = LastName;
            user.ContactNumber = ContactNumber;
            user.LanguageId = LanguageId;
            user.LinkedInProfile = LinkedInProfile;

            if (user.Email != Email)
            {
                var emailResult = await _userManager.SetEmailAsync(user, Email);

                if (!emailResult.Succeeded)
                {
                    foreach (var error in emailResult.Errors)
                    {
                        ModelState.AddModelError("Email", error.Description);
                    }

                    return View(user);
                }

                var usernameResult = await _userManager.SetUserNameAsync(user, Email);

                if (!usernameResult.Succeeded)
                {
                    foreach (var error in usernameResult.Errors)
                    {
                        ModelState.AddModelError("Email", error.Description);
                    }

                    return View(user);
                }
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(user);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}