using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SettlementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettlementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // CREATE
        public IActionResult Create()
        {
            return View();
        }

        // SAVE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SettlementPage page)
        {
            _context.SettlementPages.Add(page);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        // EDIT
        public async Task<IActionResult> Edit(int id)
        {
            var page = await _context.SettlementPages
                .Include(p => p.ContentBlocks)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (page == null)
            {
                return NotFound();
            }

            page.ContentBlocks = page.ContentBlocks
                .OrderBy(b => b.DisplayOrder)
                .ToList();

            return View(page);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SettlementPage page)
        {
            var existingPage = await _context.SettlementPages
                .Include(p => p.ContentBlocks)
                .FirstOrDefaultAsync(p => p.Id == page.Id);

            if (existingPage == null)
            {
                return NotFound();
            }

            existingPage.CategoryName = page.CategoryName;

            _context.ContentBlocks.RemoveRange(existingPage.ContentBlocks);

            foreach (var block in page.ContentBlocks)
            {
                block.Id = 0;
                block.SettlementPageId = existingPage.Id;

                _context.ContentBlocks.Add(block);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        // DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var page = await _context.SettlementPages
                .Include(p => p.ContentBlocks)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (page == null)
            {
                return NotFound();
            }

            _context.ContentBlocks.RemoveRange(page.ContentBlocks);
            _context.SettlementPages.Remove(page);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}