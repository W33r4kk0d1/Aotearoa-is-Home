using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models;
using Aotearoa_is_Home.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Areas.Family.Controllers
{
    [Area("Family")]
    [Authorize(Roles = "Family Member")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // SETTLEMENT PAGES + CHECKLIST TASKS
            var settlementPages = await _context.SettlementPages
                .Include(x => x.ChecklistTasks)
                .ToListAsync();

            // FAMILY MEMBER CHECKLIST ITEMS
            var checklistItems = await _context.ChecklistItems
                .Where(x => x.UserId == userId)
                .ToListAsync();

            // Create missing checklist items for this Family Member
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

                        _context.ChecklistItems.Add(item);
                        checklistItems.Add(item);
                    }
                }
            }

            await _context.SaveChangesAsync();


            // CALCULATE CHECKLIST PROGRESS
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

            // Same ordering as Student Home
            settlementPages = priorityPages
                .OrderByDescending(x => x.Priority)
                .ThenByDescending(x => x.Remaining)
                .ThenBy(x => x.Page.CategoryName)
                .Select(x => x.Page)
                .ToList();


            // UPCOMING EVENTS
            var events = await _context.Events
                .Include(x => x.EventProviderProfile)
                .Where(x => x.Status == "Approved")
                .OrderBy(x => x.StartDate)
                .Take(3)
                .ToListAsync();


            // HOME VIEW MODEL
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