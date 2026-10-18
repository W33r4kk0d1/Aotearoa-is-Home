
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

            // 1. Load the student's active checklists and their tasks.
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

            // 3. Get all items from the student's active checklists.
            var checklistItems = activeChecklists
                .SelectMany(c => c.Items)
                .ToList();

            // 4. Show only unfinished tasks on the Home page.
            var outstandingItems = checklistItems
                .Where(item => !item.IsCompleted)
                .OrderBy(item => item.Category)
                .ThenBy(item => item.Title)
                .ToList();

            // 5. Load settlement categories.
            var settlementPages = await _context.SettlementPages
                .Include(p => p.ChecklistTasks)
                .ToListAsync();

            // 6. Calculate progress from the student's selected tasks.
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

            // 7. Put categories needing attention first.
            settlementPages = priorityPages
                .OrderBy(x => x.Priority)
                .ThenByDescending(x => x.Remaining)
                .ThenBy(x => x.Page.CategoryName)
                .Select(x => x.Page)
                .ToList();

            // 8. Load upcoming approved events.
            var events = await _context.Events
                .Include(e => e.EventProviderProfile)
                .Where(e =>
                    e.Status == "Approved" &&
                    e.StartDate >= DateTime.Now)
                .OrderBy(e => e.StartDate)
                .Take(3)
                .ToListAsync();

            // 9. Build the Home page model.
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
