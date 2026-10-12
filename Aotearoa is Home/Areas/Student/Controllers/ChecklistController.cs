
using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var checklist = await _context.StudentChecklists
                .Where(c => c.UserId == userId && !c.IsArchived)
                .OrderByDescending(c => c.CreatedAt)
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

                _context.StudentChecklists.Add(checklist);
                await _context.SaveChangesAsync();
            }

            var tasks = await _context.ChecklistTasks
                .Include(t => t.SettlementPage)
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();

            var existingTaskIds = await _context.ChecklistItems
                .Where(i =>
                    i.UserId == userId &&
                    i.StudentChecklistId == checklist.Id &&
                    i.ChecklistTaskId.HasValue)
                .Select(i => i.ChecklistTaskId!.Value)
                .ToListAsync();

            foreach (var task in tasks)
            {
                if (existingTaskIds.Contains(task.Id))
                    continue;

                _context.ChecklistItems.Add(new ChecklistItem
                {
                    UserId = userId,
                    StudentChecklistId = checklist.Id,
                    ChecklistTaskId = task.Id,
                    Category = task.SettlementPage?.CategoryName ?? "Other",
                    Title = task.Title,
                    Description = task.Description,
                    IsCompleted = false
                });
            }

            await _context.SaveChangesAsync();

            var checklists = await _context.StudentChecklists
                .Where(c => c.UserId == userId)
                .Include(c => c.Items)
                    .ThenInclude(i => i.ChecklistTask)
                        .ThenInclude(t => t!.SettlementPage)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(checklists);
        }

        [HttpGet]
        public async Task<IActionResult> ViewChecklist(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var checklist = await _context.StudentChecklists
                .Include(c => c.Items)
                    .ThenInclude(i => i.ChecklistTask)
                        .ThenInclude(t => t!.SettlementPage)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (checklist == null)
                return NotFound();

            var settlementPages = await _context.SettlementPages
                .Include(p => p.ContentBlocks)
                .ToListAsync();

            ViewBag.SettlementPages = settlementPages;

            return View("View", checklist);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            int checklistId,
            List<int>? completedItems)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var checklist = await _context.StudentChecklists
                .FirstOrDefaultAsync(c =>
                    c.Id == checklistId &&
                    c.UserId == userId);

            if (checklist == null)
                return NotFound();

            if (checklist.IsArchived)
                return BadRequest();

            var checklistItems = await _context.ChecklistItems
                .Where(i =>
                    i.StudentChecklistId == checklistId &&
                    i.UserId == userId)
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
