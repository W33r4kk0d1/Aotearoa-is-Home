using Aotearoa_is_Home.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Aotearoa_is_Home.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // User is not logged in
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            // Get logged-in user
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                await _signInManager.SignOutAsync();

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            // Admin
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction(
                    "Index",
                    "Home",
                    new { area = "Admin" });
            }

            // Service Provider
            if (await _userManager.IsInRoleAsync(user, "Service Provider"))
            {
                return RedirectToAction(
                    "Index",
                    "Home",
                    new { area = "ServiceProvider" });
            }

            // Family Member
            if (await _userManager.IsInRoleAsync(user, "Family Member"))
            {
                return RedirectToAction(
                    "Index",
                    "Home",
                    new { area = "Family" });
            }

            // Student
            if (await _userManager.IsInRoleAsync(user, "Student"))
            {
                return RedirectToAction(
                    "Index",
                    "Home",
                    new { area = "Student" });
            }

            // Logged in but has no valid application role
            await _signInManager.SignOutAsync();

            return RedirectToAction(
                "Login",
                "Account");
        }
    }
}