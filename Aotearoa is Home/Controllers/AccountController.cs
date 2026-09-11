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

        // GET: /Account/Login
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

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return RedirectToAction(
                    "Index",
                    "Home");
            }

            ModelState.AddModelError(
                string.Empty,
                "Invalid username/email or password.");

            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            ViewBag.Languages = await _context.Languages
                .OrderBy(x => x.Name)
                .ToListAsync();

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

            var existingUser =
                await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "An account with this email already exists.");

                return View(model);
            }

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
                            EmployeeId = model.EmployeeId!,
                            DepartmentOrganisation =
                                model.DepartmentOrganisation
                        });

                    break;

                case "Event Provider":

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

                default:

                    await _userManager.DeleteAsync(user);

                    ModelState.AddModelError(
                        "AccountType",
                        "Please select a valid account type.");

                    return View(model);
            }

            await _context.SaveChangesAsync();

            await _userManager.AddToRoleAsync(
                user,
                model.AccountType);

            await _signInManager.SignInAsync(
                user,
                isPersistent: false);

            return RedirectToAction(
                "Index",
                "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(
                "Login",
                "Account");
        }

        private async Task LoadLanguages()
        {
            ViewBag.Languages = await _context.Languages
                .OrderBy(x => x.Name)
                .ToListAsync();
        }
    }
}