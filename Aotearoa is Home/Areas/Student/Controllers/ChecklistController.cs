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


        // ## Display student settlement checklist
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }


            // ## Get current student's checklist items
            var checklist = await _context.ChecklistItems
                .Where(c => c.UserId == userId)
                .ToListAsync();


            // ## Default settlement checklist
            var defaultItems = new List<ChecklistItem>
            {
                // ## Arrival and documents
                new ChecklistItem
                {
                    UserId = userId,
                    Category = "Arrival and documents",
                    Title = "Enrolment",
                    Description = "Confirm your enrolment and study details."
                },

                new ChecklistItem
                {
                    UserId = userId,
                    Category = "Arrival and documents",
                    Title = "Student Visa Status",
                    Description = "Check that your student visa details are correct and current."
                },


                // ## Accommodation
                new ChecklistItem
                {
                    UserId = userId,
                    Category = "Accommodation",
                    Title = "Find suitable accommodation",
                    Description = "Find a suitable place to live in New Zealand."
                },

                new ChecklistItem
                {
                    UserId = userId,
                    Category = "Accommodation",
                    Title = "Complete accommodation arrangements",
                    Description = "Complete the necessary arrangements for your accommodation."
                },


                // ## Find a University
                new ChecklistItem
                {
                    UserId = userId,
                    Category = "Find a University",
                    Title = "Research universities",
                    Description = "Research universities and study options in New Zealand."
                },

                new ChecklistItem
                {
                    UserId = userId,
                    Category = "Find a University",
                    Title = "Complete university application",
                    Description = "Complete the required university application process."
                },


                // ## Banking
                new ChecklistItem
                {
                    UserId = userId,
                    Category = "Banking",
                    Title = "Open a New Zealand bank account",
                    Description = "Set up a local bank account for everyday transactions."
                }
            };


            // ## Add checklist items that do not already exist
            foreach (var defaultItem in defaultItems)
            {
                var existingItem = checklist
                    .FirstOrDefault(c => c.Title == defaultItem.Title);

                if (existingItem == null)
                {
                    _context.ChecklistItems.Add(defaultItem);
                }
                else
                {
                    // ## Update old records that previously had no category
                    existingItem.Category = defaultItem.Category;

                    if (string.IsNullOrWhiteSpace(existingItem.Description))
                    {
                        existingItem.Description = defaultItem.Description;
                    }
                }
            }


            // ## Save new items and category changes
            await _context.SaveChangesAsync();


            // ## Reload checklist after changes
            checklist = await _context.ChecklistItems
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Category)
                .ThenBy(c => c.Id)
                .ToListAsync();


            return View(checklist);
        }


        // ## Update completed checklist items
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(List<int>? completedItems)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }


            // ## Get current student's checklist items
            var checklistItems = await _context.ChecklistItems
                .Where(c => c.UserId == userId)
                .ToListAsync();


            // ## Empty list if no checkboxes were selected
            completedItems ??= new List<int>();


            // ## Update completion status
            foreach (var item in checklistItems)
            {
                item.IsCompleted = completedItems.Contains(item.Id);
            }


            // ## Save checklist progress
            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }
    }
}