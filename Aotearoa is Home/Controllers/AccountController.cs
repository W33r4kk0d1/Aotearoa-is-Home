using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models;
using Aotearoa_is_Home.Models.ViewModels;
using Aotearoa_is_Home.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly UniversityDbContext _universityContext;
        private readonly IEmailService _emailService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            UniversityDbContext universityContext,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _universityContext = universityContext;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(
                model.UserNameOrEmail);

            if (user == null)
            {
                user = await _userManager.FindByNameAsync(
                    model.UserNameOrEmail);
            }

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid username/email or password.");

                return View(model);
            }

            if (await _userManager.IsInRoleAsync(user, "Service Provider")
                && !user.IsServiceProviderVerified)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your service provider account is awaiting administrator approval.");

                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (await _userManager.IsInRoleAsync(user, "Super Admin"))
                {
                    return RedirectToAction(
                        "Index",
                        "Home",
                        new { area = "Admin" });
                }

                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction(
                        "Index",
                        "Home",
                        new { area = "Admin" });
                }

                if (await _userManager.IsInRoleAsync(user, "Service Provider"))
                {
                    return RedirectToAction(
                        "Index",
                        "Home",
                        new { area = "ServiceProvider" });
                }

                if (await _userManager.IsInRoleAsync(user, "Family Member"))
                {
                    return RedirectToAction(
                        "Index",
                        "Home",
                        new { area = "Family" });
                }

                if (await _userManager.IsInRoleAsync(user, "Student"))
                {
                    return RedirectToAction(
                        "Index",
                        "Home",
                        new { area = "Student" });
                }

                await _signInManager.SignOutAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "Your account does not have a valid role.");

                return View(model);
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your account is temporarily locked. Please try again later.");

                return View(model);
            }

            ModelState.AddModelError(
                string.Empty,
                "Invalid username/email or password.");

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            await LoadLanguages();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            await LoadLanguages();

            bool isPendingStudent = false;
            Employee? verifiedEmployee = null;
            StudentFamilyMember? familyMember = null;

            var validAccountTypes = new[]
            {
                "Student",
                "Admin",
                "Service Provider",
                "Family Member"
            };

            if (!validAccountTypes.Contains(model.AccountType))
            {
                ModelState.AddModelError(
                    "AccountType",
                    "Please select a valid account type.");
            }

            // ACCOUNT-TYPE-SPECIFIC VALIDATION
            switch (model.AccountType)
            {
                case "Family Member":

                    ModelState.Remove(nameof(model.FirstName));
                    ModelState.Remove(nameof(model.LastName));
                    ModelState.Remove(nameof(model.ContactNumber));
                    ModelState.Remove(nameof(model.RelationshipToStudent));
                    ModelState.Remove(nameof(model.StudentReference));

                    var familyEmail = model.Email?.Trim();

                    if (string.IsNullOrWhiteSpace(familyEmail))
                    {
                        ModelState.AddModelError(
                            "Email",
                            "Email address is required.");

                        break;
                    }

                    model.Email = familyEmail;

                    familyMember =
                        await _context.StudentFamilyMembers
                            .FirstOrDefaultAsync(f =>
                                f.Email.ToLower() ==
                                familyEmail.ToLower());

                    if (familyMember == null)
                    {
                        ModelState.AddModelError(
                            "Email",
                            "This email is not connected with a student. Please provide a valid family member email.");

                        break;
                    }

                    if (familyMember.RegisteredUserId != null)
                    {
                        ModelState.AddModelError(
                            "Email",
                            "This family member has already registered. Please use the login page.");

                        break;
                    }

                    break;

                case "Student":

                    // Keep your existing Student validation here.
                    break;

                case "Admin":

                    // Keep your existing Admin/Employee validation here.
                    break;

                case "Service Provider":

                    // Keep your existing Service Provider validation here.
                    break;
            }

            // STOP IF VALIDATION FAILED
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // CHECK WHETHER EMAIL ALREADY HAS AN ACCOUNT

            var existingUser =
                await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "An account with this email already exists.");

                return View(model);
            }

            // PENDING STUDENT REGISTRATION

            if (isPendingStudent)
            {
                var pendingRegistration =
                    new PendingStudentRegistration
                    {
                        StudentId = model.StudentId,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        LinkedInProfile = model.LinkedInProfile,
                        SubmittedAt = DateTime.UtcNow,
                        Status = "Pending"
                    };

                _context.PendingStudentRegistrations.Add(
                    pendingRegistration);

                await _context.SaveChangesAsync();

                TempData["RegistrationSuccess"] =
                    "Your registration request has been submitted for administrator approval.";

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            // CREATE APPLICATION USER

            var user = new ApplicationUser
            {
                UserName = model.AccountType == "Family Member"
                    ? familyMember!.Email
                    : model.AccountType == "Admin"
                        ? verifiedEmployee!.Email
                        : model.Email,

                Email = model.AccountType == "Family Member"
                    ? familyMember!.Email
                    : model.AccountType == "Admin"
                        ? verifiedEmployee!.Email
                        : model.Email,

                FirstName = model.AccountType == "Family Member"
                    ? GetFirstName(familyMember!.FullName)
                    : model.AccountType == "Admin"
                        ? verifiedEmployee!.FirstName
                        : model.FirstName,

                LastName = model.AccountType == "Family Member"
                    ? GetLastName(familyMember!.FullName)
                    : model.AccountType == "Admin"
                        ? verifiedEmployee!.LastName
                        : model.LastName,

                PhoneNumber = model.AccountType == "Family Member"
                    ? familyMember!.ContactNumber
                    : model.AccountType == "Admin"
                        ? null
                        : model.ContactNumber,

                ContactNumber = model.AccountType == "Family Member"
                    ? familyMember!.ContactNumber
                    : model.AccountType == "Admin"
                        ? null
                        : model.ContactNumber,

                LinkedInProfile = model.AccountType == "Admin"
                    ? null
                    : model.LinkedInProfile,

                LanguageId = model.LanguageId,

                CreatedAt = DateTime.UtcNow,

                IsServiceProviderVerified = false
            };

            var result = await _userManager.CreateAsync(
                user,
                model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            // CREATE ROLE-SPECIFIC DATA

            switch (model.AccountType)
            {
                case "Student":

                    _context.StudentProfiles.Add(
                        new StudentProfile
                        {
                            UserId = user.Id,
                            StudentId = model.StudentId!
                        });

                    break;

                case "Admin":

                    _context.AdminProfiles.Add(
                        new AdminProfile
                        {
                            UserId = user.Id,
                            EmployeeId = model.EmployeeId!,
                            DepartmentOrganisation =
                                model.DepartmentOrganisation
                        });

                    break;

                case "Family Member":

                    if (familyMember != null)
                    {
                        familyMember.RegisteredUserId = user.Id;
                    }

                    break;

                case "Service Provider":

                    _context.EventProviderProfiles.Add(
                        new EventProviderProfile
                        {
                            UserId = user.Id,

                            OrganisationName =
                                model.OrganisationName!,

                            OrganisationType =
                                model.OrganisationType!,

                            OrganisationDescription =
                                model.OrganisationDescription,

                            OrganisationPhone =
                                model.OrganisationPhone,

                            Website =
                                model.Website,

                            OfficeAddress =
                                model.OfficeAddress,

                            SupportingInformation =
                                model.SupportingInformation
                        });

                    break;
            }

            await _context.SaveChangesAsync();

            // ADD ROLE

            var roleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    model.AccountType);

            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            // EMAIL NOTIFICATIONS

            if (model.AccountType == "Student")
            {
                try
                {
                    await _emailService.SendStudentAccountCreatedEmailAsync(
                        user.Email!,
                        user.FirstName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("========================================");
                    Console.WriteLine("STUDENT ACCOUNT CREATION EMAIL FAILED");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("========================================");

                    TempData["RegistrationWarning"] =
                        "Your account was created successfully, but the confirmation email could not be sent.";
                }
            }
            else if (model.AccountType == "Admin")
            {
                try
                {
                    await _emailService.SendAdminAccountCreatedEmailAsync(
                        user.Email!,
                        user.FirstName,
                        model.EmployeeId!,
                        model.DepartmentOrganisation ??
                        "Not specified");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("========================================");
                    Console.WriteLine("ADMIN ACCOUNT CREATION EMAIL FAILED");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("========================================");

                    TempData["RegistrationWarning"] =
                        "Your administrator account was created successfully, but the confirmation email could not be sent.";
                }
            }

            // SUCCESS MESSAGE

            if (TempData["RegistrationWarning"] == null)
            {
                if (model.AccountType == "Service Provider")
                {
                    TempData["RegistrationSuccess"] =
                        "Account created successfully. Your service provider account is awaiting administrator approval.";
                }
                else
                {
                    TempData["RegistrationSuccess"] =
                        "Account created successfully! A confirmation email has been sent.";
                }
            }

            return RedirectToAction(
                "Login",
                "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(
                "Index",
                "Home");
        }

        private async Task LoadLanguages()
        {
            ViewBag.Languages = await _context.Languages
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private static string GetFirstName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return string.Empty;
            }

            var parts = fullName.Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return parts.Length > 0
                ? parts[0]
                : string.Empty;
        }


        private static string GetLastName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return string.Empty;
            }

            var parts = fullName.Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length <= 1)
            {
                return string.Empty;
            }

            return string.Join(
                " ",
                parts.Skip(1));
        }
    }
}