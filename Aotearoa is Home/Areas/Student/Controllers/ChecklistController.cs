using Microsoft.AspNetCore.Mvc;
using Aotearoa_is_Home.Models;
using Aotearoa_is_Home.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Aotearoa_is_Home.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class ChecklistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChecklistController(
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

            var tasks = await _context.ChecklistTasks
                .Include(x => x.SettlementPage)
                .OrderBy(x => x.SettlementPage!.CategoryName)
                .ThenBy(x => x.DisplayOrder)
                .ToListAsync();

            var checklistItems = await _context.ChecklistItems
                .Where(x => x.UserId == userId)
                .ToListAsync();

            foreach (var task in tasks)
            {
                var existingItem = checklistItems
                    .FirstOrDefault(x => x.ChecklistTaskId == task.Id);

                if (existingItem == null)
                {
                    _context.ChecklistItems.Add(
                        new ChecklistItem
                        {
                            UserId = userId,
                            ChecklistTaskId = task.Id,
                            IsCompleted = false
                        });
                }
            }

            await _context.SaveChangesAsync();

            checklistItems = await _context.ChecklistItems
                .Include(x => x.ChecklistTask)
                .ThenInclude(x => x!.SettlementPage)
                .Where(x => x.UserId == userId)
                .ToListAsync();

            return View(checklistItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(List<int>? completedItems)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var checklistItems = await _context.ChecklistItems
                .Where(x => x.UserId == userId)
                .ToListAsync();

            completedItems ??= new List<int>();

            foreach (var item in checklistItems)
            {
                item.IsCompleted = completedItems.Contains(item.Id);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
