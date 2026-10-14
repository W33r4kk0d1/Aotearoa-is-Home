using Aotearoa_is_Home.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Aotearoa_is_Home.Areas.Family.Controllers
{
    [Area("Family")]
    [Authorize(Roles = "Family Member")]
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
                return NotFound();

            return View("~/Areas/Family/Views/Profile/Index.cshtml", user);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            return View("~/Areas/Family/Views/Profile/Edit.cshtml", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string FirstName,
            string LastName,
            string ContactNumber,
            int? LanguageId,
            string LinkedInProfile)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            user.FirstName = FirstName;
            user.LastName = LastName;
            user.ContactNumber = ContactNumber;
            user.LanguageId = LanguageId;
            user.LinkedInProfile = LinkedInProfile;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["ProfileMessage"] = "Your profile was updated successfully.";
                return RedirectToAction(nameof(Index), new { area = "Family" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("~/Areas/Family/Views/Profile/Edit.cshtml", user);
        }
    }
}