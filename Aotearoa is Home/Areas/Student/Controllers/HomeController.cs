
using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models;
using Aotearoa_is_Home.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // 1. Get all active checklists belonging to this student.
            var activeChecklists = await context.StudentChecklists
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsArchived)
                .ToListAsync();

            // 2. Create the default checklist if the student does not have any active checklist.
            if (!activeChecklists.Any())
            {
                var checklist = new StudentChecklist
                {
                    UserId = userId,
                    Name = "Settlement Checklist",
                    CreatedAt = DateTime.UtcNow,
                    IsArchived = false
                };

                context.StudentChecklists.Add(checklist);

                await context.SaveChangesAsync();

                activeChecklists.Add(checklist);
            }

            // 3. Get the IDs of all active checklists.
            var activeChecklistIds = activeChecklists
                .Select(x => x.Id)
                .ToList();

            // 4. Retrieve items from all active checklists belonging to the logged-in student.
            var checklistItems = await context.ChecklistItems
                .Where(x =>
                    x.UserId == userId &&
                    x.StudentChecklistId.HasValue &&
                    activeChecklistIds.Contains(x.StudentChecklistId.Value))
                .ToListAsync();


            // 5. Load all settlement categories and their tasks.
            var settlementPages = await context.SettlementPages
                .Include(x => x.ChecklistTasks)
                .ToListAsync();

            // 6. Calculate progress and recommendation priority separately for each settlement category.
            var progress =
                new Dictionary<int, ChecklistProgressViewModel>();

            var priorityPages =
                new List<(SettlementPage Page, int Priority, int Remaining)>();

            foreach (var page in settlementPages)
            {  
                var tasks = page.ChecklistTasks
                    .OrderBy(x => x.DisplayOrder)
                    .ToList();

                int total = tasks.Count;

                var taskIds = tasks
                    .Select(x => x.Id)
                    .ToHashSet();

                // A task counts as completed if completed in any active checklist.
                var completedTaskIds = checklistItems
                    .Where(x =>
                        x.ChecklistTaskId.HasValue &&
                        x.IsCompleted &&
                        taskIds.Contains(x.ChecklistTaskId.Value))
                    .Select(x => x.ChecklistTaskId.Value)
                    .ToHashSet();

                int completed = completedTaskIds.Count;
                int remaining = total - completed;

                progress[page.Id] = new ChecklistProgressViewModel
                {
                    Total = total,
                    Completed = completed
                };

                int priority;

                if (total == 0)
                {
                    priority = 3;
                }
                else if (completed > 0 && completed < total)
                {
                    priority = 0;
                }
                else if (completed == 0)
                {
                    priority = 1;
                }
                else
                {
                    priority = 2;
                }

                priorityPages.Add((page, priority, remaining));

            }

            // 7. Sort recommendations by priority.
            // Partially completed categories appear first, followed by unstarted categories and completed ones.
            settlementPages = priorityPages
                .OrderBy(x => x.Priority)
                .ThenByDescending(x => x.Remaining)
                .ThenBy(x => x.Page.CategoryName)
                .Select(x => x.Page)
                .ToList();

            // 8. Load the three earliest approved events.
            var events = await context.Events
                .Include(x => x.EventProviderProfile)
                .Where(x => x.Status == "Approved")
                .OrderBy(x => x.StartDate)
                .Take(3)
                .ToListAsync();

            // 9. Build the Home page view model.
            var model = new StudentHomeViewModel
            {
                SettlementPages = settlementPages,
                Events = events,
                ChecklistProgress = progress
            };

            return View(model);
        }
    }
}
