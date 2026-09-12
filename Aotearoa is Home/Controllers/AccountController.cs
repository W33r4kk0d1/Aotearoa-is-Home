using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models;
using Aotearoa_is_Home.Models.ViewModels;
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

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // LOGIN
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Try email first
            var user = await _userManager.FindByEmailAsync(
                model.UserNameOrEmail);

            // If not found, try username
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

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
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


        // REGISTER
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            await LoadLanguages();

            return View();
        }


        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model)
        {
            await LoadLanguages();

            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // Check account type
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

                return View(model);
            }

            // Check existing email
            var existingUser =
                await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "An account with this email already exists.");

                return View(model);
            }


            // Create Identity user
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,

                FirstName = model.FirstName,
                LastName = model.LastName,

                PhoneNumber = model.ContactNumber,

                LanguageId = model.LanguageId
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


            // Create role-specific profile
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


                case "Family Member":

                    _context.FamilyProfiles.Add(
                        new FamilyProfile
                        {
                            UserId = user.Id,

                            RelationshipToStudent =
                                model.RelationshipToStudent!,

                            StudentReference =
                                model.StudentReference!
                        });

                    break;


                case "Admin":

                    _context.AdminProfiles.Add(
                        new AdminProfile
                        {
                            UserId = user.Id,

                            EmployeeId =
                                model.EmployeeId!,

                            DepartmentOrganisation =
                                model.DepartmentOrganisation
                        });

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


            // Save profile
            await _context.SaveChangesAsync();


            // Add Identity role
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

            // Registration successful → Login page
            TempData["RegistrationSuccess"] =
                "Account created successfully!";

            return RedirectToAction(
                "Login",
                "Account");
        }

        // LOGOUT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            // Return to the existing Student Home page
            return RedirectToAction(
                "Index",
                "Home");
        }


        // LOAD LANGUAGES
        private async Task LoadLanguages()
        {
            ViewBag.Languages = await _context.Languages
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        // Access Denied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}