using Aotearoa_is_Home.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Aotearoa_is_Home.Services;

namespace Aotearoa_is_Home.Areas.Admin.Controllers
{
    [Area("Admin")]
    //dsd
    [Authorize(Roles = "Admin,Super Admin")]
    public class ServiceProviderController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<ServiceProviderController> _logger;

        public ServiceProviderController(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ILogger<ServiceProviderController> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
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

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                TempData["RegistrationWarning"] =
                    "The provider was approved, but no email address is available.";

                return RedirectToAction(nameof(Index));
            }

            // Avoid sending the approval email again if already approved.
            if (user.IsServiceProviderVerified)
            {
                TempData["RegistrationSuccess"] =
                    "This service provider has already been approved.";

                return RedirectToAction(nameof(Index));
            }

            // Update the approval status first.
            user.IsServiceProviderVerified = true;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                TempData["RegistrationWarning"] =
                    "The service provider could not be approved.";

                return RedirectToAction(nameof(Index));
            }

            // Send the approval email after the update succeeds.
            try
            {
                await _emailService.SendServiceProviderApprovalEmailAsync(
                    user.Email!,
                    user.FirstName
                );

                TempData["RegistrationSuccess"] =
                    "Service provider approved successfully. An approval email has been sent.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Service provider {UserId} was approved, but the approval email failed.",
                    user.Id
                );

                TempData["RegistrationWarning"] =
                    "Service provider approved successfully, but the approval email could not be sent.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null ||
                !await _userManager.IsInRoleAsync(user, "Service Provider"))
            {
                return NotFound();
            }

            user.IsServiceProviderVerified = false;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError(
                    "Failed to reject service provider {UserId}: {Errors}",
                    user.Id,
                    string.Join("; ", result.Errors.Select(e => e.Description)));

                TempData["RegistrationWarning"] =
                    "The service provider could not be rejected.";

                return RedirectToAction(nameof(Index));
            }

            TempData["RegistrationSuccess"] =
                "The service provider's approval has been revoked.";

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