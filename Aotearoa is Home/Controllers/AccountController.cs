using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models;
using Aotearoa_is_Home.Models.ViewModels;
using Aotearoa_is_Home.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

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

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Please verify your email address using the link sent to your email before signing in.");

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
                    {
                        // Check whether the student says they have a Student ID.
                        if (model.HasStudentId != true ||
                            string.IsNullOrWhiteSpace(model.StudentId))
                        {
                            // No Student ID: send for administrator approval.
                            isPendingStudent = true;
                            model.StudentId = null;
                            break;
                        }

                        var enteredStudentId = model.StudentId.Trim();

                        // Look up the ID in the university database.
                        var universityStudent = await _universityContext.UniversityStudents
                            .FirstOrDefaultAsync(s => s.StudentId == enteredStudentId);

                        // Missing or inactive student: send for administrator approval.
                        if (universityStudent == null ||
                            !universityStudent.IsCurrentStudent)
                        {
                            isPendingStudent = true;
                            break;
                        }

                        // Valid, active university student.
                        model.StudentId = universityStudent.StudentId;

                        break;
                    }

                case "Admin":
                    break;

                case "Service Provider":
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

            // SEND EMAIL VERIFICATION LINK
            try
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var encodedToken = WebEncoders.Base64UrlEncode(
                    Encoding.UTF8.GetBytes(token));

                var confirmationLink = Url.Action(
                    nameof(ConfirmEmail),
                    "Account",
                    new
                    {
                        userId = user.Id,
                        code = encodedToken
                    },
                    protocol: Request.Scheme);

                if (string.IsNullOrWhiteSpace(confirmationLink))
                {
                    throw new InvalidOperationException(
                        "Could not generate the email confirmation link.");
                }

                await _emailService.SendEmailConfirmationAsync(
                    user.Email!,
                    user.FirstName,
                    confirmationLink);

                TempData["RegistrationSuccess"] =
                    "Your account has been created. Please check your email and click the verification link before signing in.";
            }
            catch (Exception ex)
            {
                Console.WriteLine("EMAIL VERIFICATION EMAIL FAILED");
                Console.WriteLine(ex);

                TempData["RegistrationWarning"] =
                    "Your account was created, but the verification email could not be sent. Please contact support to request another verification email.";
            }

            return RedirectToAction(nameof(Login));

        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(
            string userId,
            string code)
        {
            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(code))
            {
                return BadRequest("Invalid email confirmation link.");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound("Account not found.");
            }

            if (user.EmailConfirmed)
            {
                TempData["RegistrationSuccess"] =
                    "Your email address has already been verified.";

                return RedirectToAction(nameof(Login));
            }

            try
            {
                var decodedCode = Encoding.UTF8.GetString(
                    WebEncoders.Base64UrlDecode(code));

                var result = await _userManager.ConfirmEmailAsync(
                    user,
                    decodedCode);

                if (result.Succeeded)
                {
                    TempData["RegistrationSuccess"] =
                        "Your email address has been verified. You can now sign in.";

                    return RedirectToAction(nameof(Login));
                }
            }
            catch (FormatException)
            {
                // The confirmation link contains an invalid token.
            }

            TempData["RegistrationWarning"] =
                "The verification link is invalid or has expired. Please request a new verification email.";

            return RedirectToAction(nameof(Login));
        }

        // CHANGE PASSWORD — SIGNED-IN USERS
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword);

            if (result.Succeeded)
            {
                // Refresh the sign-in cookie after changing the password.
                await _signInManager.RefreshSignInAsync(user);

                TempData["PasswordSuccess"] =
                    "Your password has been changed successfully.";

                return RedirectToAction(nameof(ChangePassword));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }


        // FORGOT PASSWORD — REQUEST RESET EMAIL
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(
                model.Email.Trim());

            // Always show the same confirmation message, whether or not an account exists, to avoid revealing registered email addresses.
            if (user != null &&
                await _userManager.IsEmailConfirmedAsync(user))
            {
                var token =
                    await _userManager.GeneratePasswordResetTokenAsync(user);

                var encodedToken = WebEncoders.Base64UrlEncode(
                    Encoding.UTF8.GetBytes(token));

                var resetLink = Url.Action(
                    nameof(ResetPassword),
                    "Account",
                    new
                    {
                        email = user.Email,
                        code = encodedToken
                    },
                    protocol: Request.Scheme);

                if (!string.IsNullOrWhiteSpace(resetLink))
                {
                    try
                    {
                        Console.WriteLine("PASSWORD RESET: Attempting to send email.");

                        await _emailService.SendPasswordResetEmailAsync(
                            user.Email!,
                            user.FirstName,
                            resetLink);

                        Console.WriteLine("PASSWORD RESET: Email sent successfully.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("PASSWORD RESET: EMAIL FAILED.");
                        Console.WriteLine(ex.ToString());
                    }
                }
            }

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }


        // RESET PASSWORD — FROM EMAIL LINK

        [HttpGet]
        public IActionResult ResetPassword(string? email, string? code)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(code))
            {
                TempData["PasswordResetError"] =
                    "The password reset link is invalid or incomplete.";

                return RedirectToAction(nameof(ForgotPassword));
            }

            return View(new ResetPasswordViewModel
            {
                Email = email,
                Code = code
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(
                model.Email.Trim());

            Console.WriteLine(
                $"PASSWORD RESET: User found = {user != null}");

            if (user != null)
            {
                Console.WriteLine(
                    $"PASSWORD RESET: Email confirmed = " +
                    $"{await _userManager.IsEmailConfirmedAsync(user)}");
            }

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The password reset link is invalid or has expired.");

                return View(model);
            }

            string decodedToken;

            try
            {
                decodedToken = Encoding.UTF8.GetString(
                    WebEncoders.Base64UrlDecode(model.Code));
            }
            catch (FormatException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The password reset link is invalid or has expired.");

                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(
                user,
                decodedToken,
                model.NewPassword);

            if (result.Succeeded)
            {
                TempData["PasswordSuccess"] =
                    "Your password has been reset. You can now sign in.";

                return RedirectToAction(nameof(PasswordResetConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult PasswordResetConfirmation()
        {
            return View();
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