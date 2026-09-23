using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models;
using Aotearoa_is_Home.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StudentRegistrationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UniversityDbContext _universityContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentRegistrationController(
            ApplicationDbContext context,
            UniversityDbContext universityContext,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _universityContext = universityContext;
            _userManager = userManager;
        }

        // GET: /Admin/StudentRegistration
        public async Task<IActionResult> Index()
        {
            var registrations = await _context.PendingStudentRegistrations
                .Where(r => r.Status == "Pending")
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();

            return View(registrations);
        }


        // GET: /Admin/StudentRegistration/Review/5
        [HttpGet]
        public async Task<IActionResult> Review(int id)
        {
            var registration =
                await _context.PendingStudentRegistrations
                    .FirstOrDefaultAsync(r => r.Id == id);

            if (registration == null)
            {
                return NotFound();
            }

            return View(registration);
        }


        // GET: /Admin/StudentRegistration/Approve/5
        [HttpGet]
        public async Task<IActionResult> Approve(int id)
        {
            var registration =
                await _context.PendingStudentRegistrations
                    .FirstOrDefaultAsync(r => r.Id == id);

            if (registration == null)
            {
                return NotFound();
            }

            if (registration.Status != "Pending")
            {
                TempData["RegistrationError"] =
                    "This registration has already been processed.";

                return RedirectToAction(nameof(Index));
            }

            var model = new ApproveStudentRegistrationViewModel
            {
                Id = registration.Id,
                FirstName = registration.FirstName,
                LastName = registration.LastName,
                Email = registration.Email,
                StudentId = registration.StudentId ?? string.Empty
            };

            return View(model);
        }

        // POST: /Admin/StudentRegistration/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(
            ApproveStudentRegistrationViewModel model)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("APPROVE POST WAS REACHED");
            Console.WriteLine($"Registration ID: {model.Id}");
            Console.WriteLine($"Student ID: {model.StudentId}");
            Console.WriteLine($"Temporary Password supplied: {!string.IsNullOrWhiteSpace(model.TemporaryPassword)}");
            Console.WriteLine($"Confirm Password supplied: {!string.IsNullOrWhiteSpace(model.ConfirmTemporaryPassword)}");
            Console.WriteLine($"ModelState Valid: {ModelState.IsValid}");
                        if (!ModelState.IsValid)
            {
                Console.WriteLine("APPROVE STOPPED: MODELSTATE INVALID");

                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine(
                            $"VALIDATION ERROR - {entry.Key}: {error.ErrorMessage}");
                    }
                }

                return View("Approve", model);
}

            var registration =
                await _context.PendingStudentRegistrations
                    .FirstOrDefaultAsync(r => r.Id == model.Id);

            if (registration == null)
            {
                return NotFound();
            }

            Console.WriteLine($"Registration status: {registration.Status}");

            if (registration.Status != "Pending")
            {
                TempData["RegistrationError"] =
                    "This registration has already been processed.";

                return RedirectToAction(nameof(Index));
            }

            // -------------------------------------------------
            // VERIFY STUDENT ID AGAINST UNIVERSITY DATABASE
            // -------------------------------------------------

            Console.WriteLine("APPROVE REACHED UNIVERSITY STUDENT CHECK");

            var universityStudent =
                await _universityContext.UniversityStudents
                    .FirstOrDefaultAsync(s =>
                        s.StudentId == model.StudentId);

            if (universityStudent == null)
            {
                Console.WriteLine("UNIVERSITY CHECK: STUDENT NOT FOUND");

                ModelState.AddModelError(
                    "StudentId",
                    "The Student ID could not be found in the university records.");

                return View("Approve", model);
            }

            Console.WriteLine(
                $"UNIVERSITY CHECK: Found {universityStudent.StudentId}, " +
                $"IsCurrentStudent = {universityStudent.IsCurrentStudent}");

            if (!universityStudent.IsCurrentStudent)
            {
                Console.WriteLine(
                    "APPROVE STOPPED: STUDENT IS NOT CURRENTLY ACTIVE");

                ModelState.AddModelError(
                    "StudentId",
                    $"Student ID {universityStudent.StudentId} is not currently active in the university records. " +
                    "This registration cannot be approved as a current student.");

                return View("Approve", model);
            }

            Console.WriteLine(
                "UNIVERSITY CHECK PASSED: STUDENT IS CURRENTLY ACTIVE");

            // -------------------------------------------------
            // CHECK WHETHER EMAIL ALREADY EXISTS
            // -------------------------------------------------

            var existingUser =
                await _userManager.FindByEmailAsync(registration.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "An account with this email address already exists.");

                return View("Approve", model);
            }


            // -------------------------------------------------
            // CHECK WHETHER STUDENT ID IS ALREADY USED
            // -------------------------------------------------

            var existingStudentProfile =
                await _context.StudentProfiles
                    .AnyAsync(s => s.StudentId == model.StudentId);

            if (existingStudentProfile)
            {
                ModelState.AddModelError(
                    "StudentId",
                    "This Student ID is already linked to another student account.");

                return View("Approve", model);
            }


            // -------------------------------------------------
            // CREATE APPLICATION USER
            // -------------------------------------------------

            var user = new ApplicationUser
            {
                UserName = registration.Email,
                Email = registration.Email,
                FirstName = registration.FirstName,
                LastName = registration.LastName
            };

            var userResult =
                await _userManager.CreateAsync(
                    user,
                    model.TemporaryPassword);


            if (!userResult.Succeeded)
            {
                foreach (var error in userResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View("Approve", model);
            }


            // -------------------------------------------------
            // ADD STUDENT ROLE
            // -------------------------------------------------

            var roleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    "Student");


            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                // Remove the newly created user if role creation fails.
                await _userManager.DeleteAsync(user);

                return View("Approve", model);
            }


            // -------------------------------------------------
            // CREATE STUDENT PROFILE
            // -------------------------------------------------

            var studentProfile = new StudentProfile
            {
                UserId = user.Id,
                StudentId = model.StudentId
            };

            _context.StudentProfiles.Add(studentProfile);


            // -------------------------------------------------
            // MARK REGISTRATION AS APPROVED
            // -------------------------------------------------

            registration.Status = "Approved";

            await _context.SaveChangesAsync();


            // -------------------------------------------------
            // SUCCESS
            // -------------------------------------------------

            TempData["RegistrationSuccess"] =
                $"Student registration for {registration.FirstName} {registration.LastName} was approved successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}