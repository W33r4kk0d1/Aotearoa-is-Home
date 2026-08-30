using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Areas.Admin.Controllers
{
    [Area("Admin")] // 🌟 This makes the URL start with /Admin/
    public class SettlementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettlementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // CREATE - GET
        // ============================================================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        
        // ============================================================
        // ASYNC API CHECK FOR DUPLICATE CATEGORIES
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> IsCategoryUnique(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return Json(true);
            }

            bool exists = await _context.SettlementPages
                .AnyAsync(p => p.CategoryName != null && 
                            p.CategoryName.Trim().ToLower() == categoryName.Trim().ToLower());

            return Json(!exists);
        }

        // ... Keep the rest of your CREATE-POST, EDIT, VIEW, and DELETE methods beneath this unchanged ...


        // ============================================================
        // CREATE - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SettlementPage page)
        {
            // --------------------------------------------------------
            // Category name required
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(page.CategoryName))
            {
                ModelState.AddModelError(
                    "CategoryName",
                    "Category Hub Name is required."
                );

                return View(page);
            }


            page.CategoryName =
                page.CategoryName.Trim();


            // --------------------------------------------------------
            // Check duplicate category
            // --------------------------------------------------------

            bool categoryExists =
                await _context.SettlementPages
                    .AnyAsync(p =>
                        p.CategoryName != null &&
                        p.CategoryName.Trim().ToLower()
                        == page.CategoryName.ToLower()
                    );


            if (categoryExists)
            {
                ModelState.AddModelError(
                    "CategoryName",
                    $"The settlement category \"{page.CategoryName}\" already exists."
                );

                return View(page);
            }


            // --------------------------------------------------------
            // Check duplicate topic headings
            // --------------------------------------------------------

            if (page.ContentBlocks != null)
            {
                var headings =
                    new HashSet<string>(
                        StringComparer.OrdinalIgnoreCase
                    );


                foreach (var block in page.ContentBlocks)
                {
                    if (string.Equals(
                        block.Type,
                        "heading",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        var heading =
                            block.Content?.Trim();


                        if (!string.IsNullOrWhiteSpace(heading))
                        {
                            if (!headings.Add(heading))
                            {
                                ModelState.AddModelError(
                                    "",
                                    $"You cannot have the same Topic Card Heading twice: \"{heading}\""
                                );

                                return View(page);
                            }
                        }
                    }
                }
            }


            // --------------------------------------------------------
            // Save
            // --------------------------------------------------------

            _context.SettlementPages.Add(page);

            await _context.SaveChangesAsync();


            return RedirectToAction("Index", "Home");
        }


        // ============================================================
        // VIEW
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var page =
                await _context.SettlementPages
                    .Include(p => p.ContentBlocks)
                    .FirstOrDefaultAsync(
                        p => p.Id == id
                    );


            if (page == null)
            {
                return NotFound();
            }


            page.ContentBlocks =
                page.ContentBlocks
                    .OrderBy(b => b.DisplayOrder)
                    .ToList();


            return View(page);
        }


        // ============================================================
        // EDIT - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var page =
                await _context.SettlementPages
                    .Include(p => p.ContentBlocks)
                    .FirstOrDefaultAsync(
                        p => p.Id == id
                    );


            if (page == null)
            {
                return NotFound();
            }


            page.ContentBlocks =
                page.ContentBlocks
                    .OrderBy(b => b.DisplayOrder)
                    .ToList();


            return View(page);
        }


        // ============================================================
        // EDIT - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SettlementPage page)
        {
            if (string.IsNullOrWhiteSpace(page.CategoryName))
            {
                ModelState.AddModelError(
                    "CategoryName",
                    "Category Hub Name is required."
                );

                page.ContentBlocks =
                    page.ContentBlocks?
                        .OrderBy(b => b.DisplayOrder)
                        .ToList()
                    ?? new List<ContentBlock>();

                return View(page);
            }


            page.CategoryName =
                page.CategoryName.Trim();


            // Check duplicate category except current page
            bool duplicate =
                await _context.SettlementPages
                    .AnyAsync(p =>
                        p.Id != page.Id &&
                        p.CategoryName != null &&
                        p.CategoryName.Trim().ToLower()
                        == page.CategoryName.ToLower()
                    );


            if (duplicate)
            {
                ModelState.AddModelError(
                    "CategoryName",
                    $"Another settlement category named \"{page.CategoryName}\" already exists."
                );

                page.ContentBlocks =
                    page.ContentBlocks?
                        .OrderBy(b => b.DisplayOrder)
                        .ToList()
                    ?? new List<ContentBlock>();

                return View(page);
            }


            // Find existing page
            var existingPage =
                await _context.SettlementPages
                    .Include(p => p.ContentBlocks)
                    .FirstOrDefaultAsync(
                        p => p.Id == page.Id
                    );


            if (existingPage == null)
            {
                return NotFound();
            }


            // Update name
            existingPage.CategoryName =
                page.CategoryName;


            // Delete old blocks
            _context.ContentBlocks.RemoveRange(
                existingPage.ContentBlocks
            );


            // Add new blocks
            if (page.ContentBlocks != null)
            {
                foreach (var block in page.ContentBlocks)
                {
                    block.Id = 0;

                    block.SettlementPageId =
                        existingPage.Id;

                    _context.ContentBlocks.Add(block);
                }
            }


            await _context.SaveChangesAsync();


            return RedirectToAction("Index", "Home");
        }


        // ============================================================
        // DELETE
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var page =
                await _context.SettlementPages
                    .Include(p => p.ContentBlocks)
                    .FirstOrDefaultAsync(
                        p => p.Id == id
                    );


            if (page == null)
            {
                return NotFound();
            }


            _context.ContentBlocks.RemoveRange(
                page.ContentBlocks
            );


            _context.SettlementPages.Remove(page);


            await _context.SaveChangesAsync();


            return RedirectToAction(
                "Index",
                "Home"
            );
        }
    }
}