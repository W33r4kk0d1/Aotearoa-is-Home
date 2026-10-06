using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Aotearoa_is_Home.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
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

        // ============================================================
        // STUDENT PROFILE
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            var familyMembers = await _context.StudentFamilyMembers
                .Where(f => f.StudentUserId == user.Id)
                .OrderBy(f => f.FullName)
                .ToListAsync();

            ViewBag.FamilyMembers = familyMembers;

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

            user.FirstName = FirstName.Trim();
            user.LastName = LastName.Trim();
            user.ContactNumber = ContactNumber?.Trim();
            user.LanguageId = LanguageId;
            user.LinkedInProfile = LinkedInProfile?.Trim();

            if (!string.Equals(user.Email, Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailResult = await _userManager.SetEmailAsync(user, Email.Trim());

                if (!emailResult.Succeeded)
                {
                    foreach (var error in emailResult.Errors)
                    {
                        ModelState.AddModelError("Email", error.Description);
                    }

                    return View(user);
                }

                var usernameResult = await _userManager.SetUserNameAsync(user, Email.Trim());

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


        // ============================================================
        // ADD FAMILY MEMBER
        // ============================================================

        [HttpGet]
        public IActionResult AddFamilyMember()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFamilyMember(
            [Bind("FullName,RelationshipToStudent,Email,ContactNumber,DateOfBirth,Gender,CountryOfCitizenship,Notes")]
            StudentFamilyMember model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            var email = model.Email?.Trim().ToLowerInvariant();

            // --------------------------------------------------------
            // Prevent student from adding themselves
            // --------------------------------------------------------

            if (string.Equals(
                email,
                user.Email?.Trim().ToLowerInvariant(),
                StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    "Email",
                    "You cannot use your own email address for a family member.");
            }

            // --------------------------------------------------------
            // Prevent duplicate family-member email
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(email))
            {
                var familyEmailExists = await _context.StudentFamilyMembers
                    .AnyAsync(f =>
                        f.Email.ToLower() == email);

                if (familyEmailExists)
                {
                    ModelState.AddModelError(
                        "Email",
                        "This email address has already been added as a family member.");
                }

                // ----------------------------------------------------
                // Prevent using an existing application account
                // ----------------------------------------------------

                var existingUser = await _userManager.FindByEmailAsync(email);

                if (existingUser != null)
                {
                    ModelState.AddModelError(
                        "Email",
                        "This email address already belongs to a registered user.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.StudentUserId = user.Id;
            model.Email = email!;
            model.FullName = model.FullName.Trim();
            model.RelationshipToStudent = model.RelationshipToStudent.Trim();
            model.ContactNumber = model.ContactNumber?.Trim();
            model.CountryOfCitizenship = model.CountryOfCitizenship?.Trim();
            model.Notes = model.Notes?.Trim();
            model.RegisteredUserId = null;
            model.CreatedAt = DateTime.UtcNow;

            _context.StudentFamilyMembers.Add(model);
            await _context.SaveChangesAsync();

            TempData["FamilyMessage"] =
                $"{model.FullName} has been added to your family members.";

            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // EDIT FAMILY MEMBER
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> EditFamilyMember(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            var familyMember = await _context.StudentFamilyMembers
                .FirstOrDefaultAsync(f =>
                    f.Id == id &&
                    f.StudentUserId == user.Id);

            if (familyMember == null)
            {
                return NotFound();
            }

            return View(familyMember);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFamilyMember(
            int id,
            [Bind("Id,FullName,RelationshipToStudent,Email,ContactNumber,DateOfBirth,Gender,CountryOfCitizenship,Notes")]
            StudentFamilyMember model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            var familyMember = await _context.StudentFamilyMembers
                .FirstOrDefaultAsync(f =>
                    f.Id == id &&
                    f.StudentUserId == user.Id);

            if (familyMember == null)
            {
                return NotFound();
            }

            var email = model.Email?.Trim().ToLowerInvariant();

            // --------------------------------------------------------
            // If already registered, do not allow email to change
            // --------------------------------------------------------

            if (familyMember.RegisteredUserId != null &&
                !string.Equals(
                    familyMember.Email,
                    email,
                    StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    "Email",
                    "The email address cannot be changed because this family member has already registered.");
            }

            // --------------------------------------------------------
            // Prevent student from using their own email
            // --------------------------------------------------------

            if (string.Equals(
                email,
                user.Email?.Trim().ToLowerInvariant(),
                StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    "Email",
                    "You cannot use your own email address for a family member.");
            }

            // --------------------------------------------------------
            // Check duplicate email
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(email))
            {
                var duplicateEmail = await _context.StudentFamilyMembers
                    .AnyAsync(f =>
                        f.Id != id &&
                        f.Email.ToLower() == email);

                if (duplicateEmail)
                {
                    ModelState.AddModelError(
                        "Email",
                        "This email address has already been added as another family member.");
                }

                // Only check ApplicationUser if email is being changed
                if (!string.Equals(
                    familyMember.Email,
                    email,
                    StringComparison.OrdinalIgnoreCase))
                {
                    var existingUser = await _userManager.FindByEmailAsync(email);

                    if (existingUser != null)
                    {
                        ModelState.AddModelError(
                            "Email",
                            "This email address already belongs to a registered user.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                model.RegisteredUserId = familyMember.RegisteredUserId;
                model.StudentUserId = familyMember.StudentUserId;

                return View(model);
            }

            // --------------------------------------------------------
            // Update allowed information
            // --------------------------------------------------------

            familyMember.FullName = model.FullName.Trim();
            familyMember.RelationshipToStudent =
                model.RelationshipToStudent.Trim();

            // Only update email if this family member has not registered
            if (familyMember.RegisteredUserId == null)
            {
                familyMember.Email = email!;
            }

            familyMember.ContactNumber = model.ContactNumber?.Trim();
            familyMember.DateOfBirth = model.DateOfBirth;
            familyMember.Gender = model.Gender;
            familyMember.CountryOfCitizenship =
                model.CountryOfCitizenship?.Trim();
            familyMember.Notes = model.Notes?.Trim();

            await _context.SaveChangesAsync();

            TempData["FamilyMessage"] =
                $"{familyMember.FullName}'s details have been updated.";

            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // DELETE FAMILY MEMBER
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFamilyMember(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            var familyMember = await _context.StudentFamilyMembers
                .FirstOrDefaultAsync(f =>
                    f.Id == id &&
                    f.StudentUserId == user.Id);

            if (familyMember == null)
            {
                return NotFound();
            }

            // --------------------------------------------------------
            // Registered family members cannot be deleted
            // --------------------------------------------------------

            if (familyMember.RegisteredUserId != null)
            {
                TempData["FamilyError"] =
                    "This family member cannot be deleted because they already have an existing user account. Please contact an administrator for assistance.";

                return RedirectToAction(nameof(Index));
            }

            _context.StudentFamilyMembers.Remove(familyMember);

            await _context.SaveChangesAsync();

            TempData["FamilyMessage"] =
                $"{familyMember.FullName} has been removed from your family members.";

            return RedirectToAction(nameof(Index));
        }
    }
}