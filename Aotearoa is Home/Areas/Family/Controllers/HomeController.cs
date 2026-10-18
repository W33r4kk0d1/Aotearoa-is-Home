
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
                return Unauthorized();

            // 1. Load active checklists belonging to this Family Member.
            var activeChecklists = await _context.StudentChecklists
                .Where(c => c.UserId == userId && !c.IsArchived)
                .Include(c => c.Items)
                    .ThenInclude(i => i.ChecklistTask)
                        .ThenInclude(t => t!.SettlementPage)
                .ToListAsync();

            // 2. Create a default checklist if none exists.
            if (!activeChecklists.Any())
            {
                var checklist = new StudentChecklist
                {
                    UserId = userId,
                    Name = "Settlement Checklist",
                    CreatedAt = DateTime.UtcNow,
                    IsArchived = false
                };

                _context.StudentChecklists.Add(checklist);
                await _context.SaveChangesAsync();

                activeChecklists.Add(checklist);
            }

            // 3. Collect items from this Family Member's active checklists.
            var checklistItems = activeChecklists
                .SelectMany(c => c.Items)
                .ToList();

            var outstandingItems = checklistItems
                .Where(item => !item.IsCompleted)
                .OrderBy(item => item.Category)
                .ThenBy(item => item.Title)
                .ToList();

            // 4. Load settlement pages and their tasks.
            var settlementPages = await _context.SettlementPages
                .Include(p => p.ChecklistTasks)
                .ToListAsync();

            // 5. Calculate category progress.
            var progress =
                new Dictionary<int, ChecklistProgressViewModel>();

            var priorityPages =
                new List<(SettlementPage Page, int Priority, int Remaining)>();

            foreach (var page in settlementPages)
            {
                var selectedItems = checklistItems
                    .Where(item =>
                        item.ChecklistTask?.SettlementPageId == page.Id)
                    .ToList();

                int total = selectedItems.Count;
                int completed = selectedItems.Count(item => item.IsCompleted);
                int remaining = total - completed;

                progress[page.Id] = new ChecklistProgressViewModel
                {
                    Total = total,
                    Completed = completed
                };

                int priority;

                if (total == 0)
                    priority = 3;
                else if (completed > 0 && completed < total)
                    priority = 0;
                else if (completed == 0)
                    priority = 1;
                else
                    priority = 2;

                priorityPages.Add((page, priority, remaining));
            }

            // 6. Order categories: partial, not started, completed, no tasks.
            settlementPages = priorityPages
                .OrderBy(x => x.Priority)
                .ThenByDescending(x => x.Remaining)
                .ThenBy(x => x.Page.CategoryName)
                .Select(x => x.Page)
                .ToList();

            // 7. Load upcoming approved events.
            var events = await _context.Events
                .Include(e => e.EventProviderProfile)
                .Where(e =>
                    e.Status == "Approved" &&
                    e.StartDate >= DateTime.Now)
                .OrderBy(e => e.StartDate)
                .Take(3)
                .ToListAsync();

            // 8. Build the Home page model.
            var model = new StudentHomeViewModel
            {
                SettlementPages = settlementPages,
                Events = events,
                ChecklistProgress = progress,
                OutstandingChecklistItems = outstandingItems
            };

            return View(model);
        }
    }
}
