
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

        private string? CurrentUserId =>
            _userManager.GetUserId(User);

        // Loads only currently available tasks.
        private async Task LoadAvailableTasksAsync(
            IEnumerable<int>? selectedTaskIds = null)
        {
            var tasks = await _context.ChecklistTasks
                .AsNoTracking()
                .Include(t => t.SettlementPage)
                .OrderBy(t => t.SettlementPage!.CategoryName)
                .ThenBy(t => t.DisplayOrder)
                .ToListAsync();

            ViewBag.AvailableTasks = tasks;
            ViewBag.SelectedTaskIds =
                (selectedTaskIds ?? Enumerable.Empty<int>())
                .ToHashSet();
        }

        // INDEX: show only this student's active checklists.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = CurrentUserId;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var checklists = await _context.StudentChecklists
                .AsNoTracking()
                .Where(c => c.UserId == userId && !c.IsArchived)
                .Include(c => c.Items)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(checklists);
        }

        // CREATE: show form.
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (string.IsNullOrEmpty(CurrentUserId))
                return Unauthorized();

            await LoadAvailableTasksAsync();

            return View();
        }

        // CREATE: save a checklist with selected tasks.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string name,
            List<int>? taskIds)
        {
            var userId = CurrentUserId;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            name = (name ?? string.Empty).Trim();
            taskIds = (taskIds ?? new List<int>())
                .Distinct()
                .ToList();

            if (string.IsNullOrWhiteSpace(name) || name.Length > 150)
            {
                ModelState.AddModelError(
                    "name",
                    "Enter a checklist name of no more than 150 characters.");
            }

            var selectedTasks = await _context.ChecklistTasks
                .Include(t => t.SettlementPage)
                .Where(t => taskIds.Contains(t.Id))
                .ToListAsync();

            // Reject IDs that do not refer to current tasks.
            if (selectedTasks.Count != taskIds.Count)
            {
                ModelState.AddModelError(
                    "",
                    "Some selected tasks are no longer available.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAvailableTasksAsync(taskIds);
                return View();
            }

            var checklist = new StudentChecklist
            {
                UserId = userId,
                Name = name,
                CreatedAt = DateTime.UtcNow,
                IsArchived = false
            };

            foreach (var task in selectedTasks)
            {
                checklist.Items.Add(new ChecklistItem
                {
                    UserId = userId,
                    ChecklistTaskId = task.Id,
                    Category =
                        task.SettlementPage?.CategoryName ?? "Other",
                    Title = task.Title,
                    Description = task.Description,
                    IsCompleted = false
                });
            }

            _context.StudentChecklists.Add(checklist);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // VIEW: display one checklist owned by the current student.
        [HttpGet]
        public async Task<IActionResult> ViewChecklist(int id)
        {
            var userId = CurrentUserId;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var checklist = await _context.StudentChecklists
                .Include(c => c.Items)
                    .ThenInclude(i => i.ChecklistTask)
                        .ThenInclude(t => t!.SettlementPage)
                .FirstOrDefaultAsync(c =>
                    c.Id == id && c.UserId == userId);

            if (checklist == null)
                return NotFound();

            // Load current settlement information for the details panel.
            ViewBag.SettlementPages = await _context.SettlementPages
                .AsNoTracking()
                .Include(p => p.ContentBlocks)
                .ToListAsync();
            

            var changed = false;

            foreach (var item in checklist.Items)
            {
                // A null task relationship means this is a historical entry.
                if (item.ChecklistTask is not { } task)
                    continue;

                var currentCategory =
                    task.SettlementPage?.CategoryName ?? "Other";

                if (item.Title != task.Title)
                {
                    item.Title = task.Title;
                    changed = true;
                }

                if (item.Description != task.Description)
                {
                    item.Description = task.Description;
                    changed = true;
                }

                if (item.Category != currentCategory)
                {
                    item.Category = currentCategory;
                    changed = true;
                }
            }

            if (changed)
            {
                await _context.SaveChangesAsync();
            }

            return View("View", checklist);
        }

        // CUSTOMIZE: show the form for an owned checklist.
        [HttpGet]
        public async Task<IActionResult> Customize(int id)
        {
            var userId = CurrentUserId;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var checklist = await _context.StudentChecklists
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c =>
                    c.Id == id && c.UserId == userId);

            if (checklist == null)
                return NotFound();

            if (checklist.IsArchived)
                return BadRequest("Archived checklists cannot be edited.");

            var selectedIds = checklist.Items
                .Where(i => i.ChecklistTaskId.HasValue)
                .Select(i => i.ChecklistTaskId!.Value)
                .ToList();

            await LoadAvailableTasksAsync(selectedIds);

            return View(checklist);
        }

        // CUSTOMIZE: update the name and selected active tasks.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Customize(
            int id,
            string name,
            List<int>? taskIds)
        {
            var userId = CurrentUserId;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var checklist = await _context.StudentChecklists
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c =>
                    c.Id == id && c.UserId == userId);

            if (checklist == null)
                return NotFound();

            if (checklist.IsArchived)
                return BadRequest("Archived checklists cannot be edited.");

            name = (name ?? string.Empty).Trim();
            taskIds = (taskIds ?? new List<int>())
                .Distinct()
                .ToList();

            if (string.IsNullOrWhiteSpace(name) || name.Length > 150)
            {
                ModelState.AddModelError(
                    "name",
                    "Enter a checklist name of no more than 150 characters.");
            }

            var selectedTasks = await _context.ChecklistTasks
                .Include(t => t.SettlementPage)
                .Where(t => taskIds.Contains(t.Id))
                .ToListAsync();

            if (selectedTasks.Count != taskIds.Count)
            {
                ModelState.AddModelError(
                    "",
                    "Some selected tasks are no longer available.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAvailableTasksAsync(taskIds);
                return View(checklist);
            }

            checklist.Name = name;

            var selectedIds = taskIds.ToHashSet();

            // Remove active task entries that the student deselected.
            // Keep historical entries whose original task no longer exists.
            var removedItems = checklist.Items
                .Where(i =>
                    i.ChecklistTaskId.HasValue &&
                    !selectedIds.Contains(i.ChecklistTaskId.Value))
                .ToList();

            _context.ChecklistItems.RemoveRange(removedItems);

            var existingByTaskId = checklist.Items
                .Where(i => i.ChecklistTaskId.HasValue)
                .GroupBy(i => i.ChecklistTaskId!.Value)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var task in selectedTasks)
            {
                if (existingByTaskId.TryGetValue(task.Id, out var item))
                {
                    // Keep completion status; refresh current task details.
                    item.Title = task.Title;
                    item.Description = task.Description;
                    item.Category =
                        task.SettlementPage?.CategoryName ?? "Other";
                }
                else
                {
                    checklist.Items.Add(new ChecklistItem
                    {
                        UserId = userId,
                        StudentChecklistId = checklist.Id,
                        ChecklistTaskId = task.Id,
                        Category =
                            task.SettlementPage?.CategoryName ?? "Other",
                        Title = task.Title,
                        Description = task.Description,
                        IsCompleted = false
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(ViewChecklist),
                new { id = checklist.Id });
        }

        // UPDATE: save completion status for an owned checklist.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            int checklistId,
            List<int>? completedItems)
        {
            var userId = CurrentUserId;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var checklist = await _context.StudentChecklists
                .FirstOrDefaultAsync(c =>
                    c.Id == checklistId &&
                    c.UserId == userId);

            if (checklist == null)
                return NotFound();

            if (checklist.IsArchived)
                return BadRequest("Archived checklists cannot be updated.");

            var items = await _context.ChecklistItems
                .Where(i =>
                    i.StudentChecklistId == checklistId &&
                    i.UserId == userId)
                .ToListAsync();

            var completedIds = (completedItems ?? new List<int>())
                .ToHashSet();

            // Only items belonging to this checklist can be changed.
            foreach (var item in items)
                item.IsCompleted = completedIds.Contains(item.Id);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(ViewChecklist),
                new { id = checklist.Id });
        }

        // DELETE: show confirmation.
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = CurrentUserId;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var checklist = await _context.StudentChecklists
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.Id == id && c.UserId == userId);

            if (checklist == null)
                return NotFound();

            return View(checklist);
        }

        // DELETE: remove this student's checklist and its own items.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = CurrentUserId;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var checklist = await _context.StudentChecklists
                .FirstOrDefaultAsync(c =>
                    c.Id == id && c.UserId == userId);

            if (checklist == null)
                return NotFound();

            var items = await _context.ChecklistItems
                .Where(i =>
                    i.StudentChecklistId == checklist.Id &&
                    i.UserId == userId)
                .ToListAsync();

            _context.ChecklistItems.RemoveRange(items);
            _context.StudentChecklists.Remove(checklist);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
