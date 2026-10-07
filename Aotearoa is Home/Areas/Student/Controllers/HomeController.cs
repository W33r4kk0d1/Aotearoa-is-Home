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

            var settlementPages = await context.SettlementPages
                .Include(x => x.ChecklistTasks)
                .ToListAsync();

            var checklistItems = await context.ChecklistItems
                .Where(x => x.UserId == userId)
                .ToListAsync();

            foreach (var page in settlementPages)
            {
                foreach (var task in page.ChecklistTasks)
                {
                    if (!checklistItems.Any(x =>
                        x.ChecklistTaskId == task.Id))
                    {
                        var item = new ChecklistItem
                        {
                            UserId = userId,
                            ChecklistTaskId = task.Id,
                            IsCompleted = false
                        };

                        context.ChecklistItems.Add(item);
                        checklistItems.Add(item);
                    }
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

                var completed = tasks.Count(task =>
                    checklistItems.Any(item =>
                        item.ChecklistTaskId == task.Id &&
                        item.IsCompleted));

                var remaining = total - completed;

                progress[page.Id] =
                    new ChecklistProgressViewModel
                    {
                        Total = total,
                        Completed = completed
                    };

                int priority;

                if (total == 0)
                {
                    priority = 0;
                }
                else if (completed == total)
                {
                    priority = -100;
                }
                else if (completed == 0)
                {
                    priority = 0;
                }
                else
                {
                    priority = 100 + remaining;
                }

                priorityPages.Add(
                    (page, priority, remaining));
            }

            settlementPages = priorityPages
                .OrderByDescending(x => x.Priority)
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
