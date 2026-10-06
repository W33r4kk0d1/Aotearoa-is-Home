using Aotearoa_is_Home.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Aotearoa_is_Home.Areas.Admin.Controllers
{
    [Area("Admin")]
    //dsd
    [Authorize(Roles = "Admin,Super Admin")]
    public class ServiceProviderController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ServiceProviderController(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(
            string? search,
            string? status)
        {
            var users = await _userManager.GetUsersInRoleAsync("Service Provider");

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                users = users
                    .Where(u =>
                        ($"{u.FirstName} {u.LastName}")
                            .Contains(search, StringComparison.OrdinalIgnoreCase)
                        || (u.Email ?? "")
                            .Contains(search, StringComparison.OrdinalIgnoreCase)
                        || (u.ContactNumber ?? "")
                            .Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (status == "Approved")
            {
                users = users
                    .Where(u => u.IsServiceProviderVerified)
                    .ToList();
            }
            else if (status == "Pending")
            {
                users = users
                    .Where(u => !u.IsServiceProviderVerified)
                    .ToList();
            }

            ViewBag.Search = search;
            ViewBag.Status = status;

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if (!await _userManager.IsInRoleAsync(user, "Service Provider"))
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if (!await _userManager.IsInRoleAsync(user, "Service Provider"))
            {
                return NotFound();
            }

            user.IsServiceProviderVerified = true;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.IsServiceProviderVerified = false;

            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if (!await _userManager.IsInRoleAsync(user, "Service Provider"))
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}