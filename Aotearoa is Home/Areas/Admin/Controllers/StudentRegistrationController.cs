using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models;
using Aotearoa_is_Home.Models.ViewModels;
using Aotearoa_is_Home.Services;
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
        private readonly IEmailService _emailService;

        public StudentRegistrationController(
            ApplicationDbContext context,
            UniversityDbContext universityContext,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService)
        {
            _context = context;
            _universityContext = universityContext;
            _userManager = userManager;
            _emailService = emailService;
        }

        // GET: /Admin/StudentRegistration
        public async Task<IActionResult> Index(
            string? search,
            string? filter,
            DateTime? fromDate,
            DateTime? toDate)
        {
            // Get all pending registrations
            var query = _context.PendingStudentRegistrations
                .Where(r => r.Status == "Pending")
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(r =>
                    r.FirstName.Contains(search) ||
                    r.LastName.Contains(search) ||
                    r.Email.Contains(search) ||
                    (r.StudentId != null && r.StudentId.Contains(search)));
            }

            // DATE FILTER
            if (fromDate.HasValue)
            {
                var from = fromDate.Value.Date;

                query = query.Where(r =>
                    r.SubmittedAt >= from);
            }

            if (toDate.HasValue)
            {
                // Include the entire selected "To" date.
                var to = toDate.Value.Date.AddDays(1);

                query = query.Where(r =>
                    r.SubmittedAt < to);
            }

            // LOAD RESULTS
            var registrations = await query
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();

            // INACTIVE STUDENTS
            var studentIds = registrations
                .Where(r => !string.IsNullOrWhiteSpace(r.StudentId))
                .Select(r => r.StudentId!)
                .Distinct()
                .ToList();

            var inactiveStudentIds = await _universityContext.UniversityStudents
                .Where(s =>
                    studentIds.Contains(s.StudentId) &&
                    !s.IsCurrentStudent)
                .Select(s => s.StudentId)
                .ToListAsync();

            // STATUS FILTER
            if (string.Equals(filter, "new", StringComparison.OrdinalIgnoreCase))
            {
                // New users = pre-arrival registrations
                // where no Student ID was provided.
                registrations = registrations
                    .Where(r => string.IsNullOrWhiteSpace(r.StudentId))
                    .ToList();
            }
            else if (string.Equals(filter, "inactive", StringComparison.OrdinalIgnoreCase))
            {
                registrations = registrations
                    .Where(r =>
                        !string.IsNullOrWhiteSpace(r.StudentId) &&
                        inactiveStudentIds.Contains(r.StudentId))
                    .ToList();
            }

            // SEND FILTER VALUES BACK TO VIEW
            ViewBag.Search = search;
            ViewBag.Filter = filter;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

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
                TempData["RegistrationError"] = "This registration has already been processed.";

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

            if (!string.Equals(
                        model.TemporaryPassword,
                        model.ConfirmTemporaryPassword,
                        StringComparison.Ordinal))
                {
                    ModelState.AddModelError(
                        "ConfirmTemporaryPassword",
                        "The passwords do not match.");
                }

                if (!ModelState.IsValid)
                {
                    var invalidRegistration =
                        await _context.PendingStudentRegistrations
                            .FirstOrDefaultAsync(r => r.Id == model.Id);

                    if (invalidRegistration == null)
                    {
                        return NotFound();
                    }

                    model.FirstName = invalidRegistration.FirstName;
                    model.LastName = invalidRegistration.LastName;
                    model.Email = invalidRegistration.Email;

                    return View("Approve", model);
                }

            // GET REGISTRATION
            var registration =
                await _context.PendingStudentRegistrations
                    .FirstOrDefaultAsync(r => r.Id == model.Id);

            if (registration == null)
            {
                return NotFound();
            }

            // CHECK REGISTRATION STATUS
            if (registration.Status != "Pending")
            {
                TempData["RegistrationError"] = "This registration has already been processed.";

                return RedirectToAction(nameof(Index));
            }


            // NORMALISE STUDENT ID
            var studentId = string.IsNullOrWhiteSpace(model.StudentId)
                ? null
                : model.StudentId.Trim();


            // VERIFY STUDENT ID AND EMAIL
            UniversityStudent? universityStudent = null;

            if (!string.IsNullOrWhiteSpace(studentId))
            {
                universityStudent =
                    await _universityContext.UniversityStudents
                        .FirstOrDefaultAsync(s => s.StudentId == studentId);

                // Student ID does not exist
                if (universityStudent == null)
                {
                    ModelState.AddModelError(
                        "StudentId",
                        "Invalid Student ID. The Student ID could not be found in the university records.");

                    // Keep the original registration details visible
                    model.FirstName = registration.FirstName;
                    model.LastName = registration.LastName;
                    model.Email = registration.Email;
                    model.StudentId = studentId;

                    return View("Approve", model);
                }

                // Compare the registration email with BOTH
                // possible university email fields.
                bool emailMatches =
                    string.Equals(
                        registration.Email?.Trim(),
                        universityStudent.StudentEmail?.Trim(),
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    string.Equals(
                        registration.Email?.Trim(),
                        universityStudent.ApplicationEmail?.Trim(),
                        StringComparison.OrdinalIgnoreCase);

                if (!emailMatches)
                {
                    ModelState.AddModelError(
                        "StudentId",
                        "The Student ID is valid, but the email address does not match the university record.");

                    // Keep the original registration details visible
                    model.FirstName = registration.FirstName;
                    model.LastName = registration.LastName;
                    model.Email = registration.Email;
                    model.StudentId = studentId;

                    return View("Approve", model);
                }

                // Student ID and email match.
                // If the student is inactive, show the warning,
                // but allow the administrator to continue.
                if (!universityStudent.IsCurrentStudent)
                {
                    ViewBag.InactiveStudentWarning =
                        $"Student ID {universityStudent.StudentId} is not currently active. " +
                        "Administrator approval is required before this account can be created.";
                }
            }


            // CREATE APPLICATION USER
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


            // ADD STUDENT ROLE
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

                await _userManager.DeleteAsync(user);

                return View("Approve", model);
            }


            // CREATE STUDENT PROFILE
            //
            // Only create a StudentProfile when a Student ID is available.
            // A pre-arrival student may be approved without having a Student ID yet.
            //

            if (!string.IsNullOrWhiteSpace(studentId))
            {
                var studentProfile = new StudentProfile
                {
                    UserId = user.Id,
                    StudentId = studentId
                };

                _context.StudentProfiles.Add(studentProfile);
            }


            // MARK REGISTRATION AS APPROVED
            registration.Status = "Approved";

            await _context.SaveChangesAsync();


            // SEND APPROVAL EMAIL
            try
            {
                await _emailService.SendStudentApprovalEmailAsync(
                    registration.Email,
                    registration.FirstName,
                    model.TemporaryPassword);
            }
            catch (Exception ex)
            {
                Console.WriteLine("========================================");
                Console.WriteLine("APPROVAL EMAIL FAILED");
                Console.WriteLine(ex.Message);
                Console.WriteLine("========================================");

                TempData["RegistrationWarning"] =
                    $"The student account was approved successfully, but the email could not be sent. Error: {ex.Message}";

                return RedirectToAction(nameof(Index));
            }


            // SUCCESS
            TempData["RegistrationSuccess"] =
                $"Student registration for {registration.FirstName} {registration.LastName} was approved successfully, and the login details were emailed to {registration.Email}.";

            return RedirectToAction(nameof(Index));
        }
        
    }
}