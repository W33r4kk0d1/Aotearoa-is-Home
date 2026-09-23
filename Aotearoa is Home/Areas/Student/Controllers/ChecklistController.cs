using Microsoft.AspNetCore.Mvc;
using Aotearoa_is_Home.Models;
using Aotearoa_is_Home.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Reflection.Metadata.Ecma335;

namespace Aotearoa_is_Home.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class ChecklistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChecklistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var checklist = await _context.ChecklistItems
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!checklist.Any())
            {
                checklist = new List<ChecklistItem>
        {
            new ChecklistItem
            {
                UserId = userId,
                Title = "Find suitable accommodation",
                Description = "Find a suitable place to live in New Zealand."
            },

            new ChecklistItem
            {
                UserId = userId,
                Title = "Open a New Zealand bank account",
                Description = "Set up a local bank account for everyday transactions."
            }
        };

                _context.ChecklistItems.AddRange(checklist);

                await _context.SaveChangesAsync();
            }

            return View(checklist);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(List<int> completedItems)
        {
            var userId = _userManager.GetUserId(User);
            var checklistItems = await _context.ChecklistItems
                .Where(c => c.UserId == userId)
                .ToListAsync();
            foreach (var item in checklistItems)
            {
                item.IsCompleted = completedItems.Contains(item.Id);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
