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

            var checklist = await context.StudentChecklists
                .Where(x => x.UserId == userId && !x.IsArchived)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (checklist == null)
            {
                checklist = new StudentChecklist
                {
                    UserId = userId,
                    Name = "Settlement Checklist",
                    CreatedAt = DateTime.UtcNow,
                    IsArchived = false
                };

                context.StudentChecklists.Add(checklist);
                await context.SaveChangesAsync();
            }

            var settlementPages = await context.SettlementPages
                .Include(x => x.ChecklistTasks)
                .ToListAsync();

            var checklistItems = await context.ChecklistItems
                .Where(x =>
                    x.UserId == userId &&
                    x.StudentChecklistId == checklist.Id)
                .ToListAsync();

            var existingTaskIds = checklistItems
                .Where(x => x.ChecklistTaskId.HasValue)
                .Select(x => x.ChecklistTaskId!.Value)
                .ToHashSet();

            foreach (var page in settlementPages)
            {
                foreach (var task in page.ChecklistTasks)
                {
                    if (existingTaskIds.Contains(task.Id))
                    {
                        continue;
                    }

                    var item = new ChecklistItem
                    {
                        UserId = userId,
                        StudentChecklistId = checklist.Id,
                        ChecklistTaskId = task.Id,
                        Category = page.CategoryName,
                        Title = task.Title,
                        Description = task.Description,
                        IsCompleted = false
                    };

                    context.ChecklistItems.Add(item);
                    checklistItems.Add(item);
                    existingTaskIds.Add(task.Id);
                }
            }

            await context.SaveChangesAsync();

            var progress =
                new Dictionary<int, ChecklistProgressViewModel>();

            var priorityPages =
                new List<(SettlementPage Page, int Priority, int Remaining)>();

            foreach (var page in settlementPages)
            {
                var tasks = page.ChecklistTasks
                    .OrderBy(x => x.DisplayOrder)
                    .ToList();

                var total = tasks.Count;

                var taskIds = tasks
                    .Select(x => x.Id)
                    .ToHashSet();

                var pageItems = checklistItems
                    .Where(x =>
                        x.ChecklistTaskId.HasValue &&
                        taskIds.Contains(x.ChecklistTaskId.Value))
                    .GroupBy(x => x.ChecklistTaskId!.Value)
                    .Select(x => x.First())
                    .ToList();

                var completed = pageItems.Count(x => x.IsCompleted);
                var remaining = total - completed;

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

            settlementPages = priorityPages
                .OrderBy(x => x.Priority)
                .ThenByDescending(x => x.Remaining)
                .ThenBy(x => x.Page.CategoryName)
                .Select(x => x.Page)
                .ToList();

            var events = await context.Events
                .Include(x => x.EventProviderProfile)
                .Where(x => x.Status == "Approved")
                .OrderBy(x => x.StartDate)
                .Take(3)
                .ToListAsync();

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