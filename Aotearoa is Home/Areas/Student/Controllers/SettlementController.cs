using Aotearoa_is_Home.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Areas.Student.Controllers
{
    [Area("Student")]
    public class SettlementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettlementController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var pages = await _context.SettlementPages
                .OrderBy(p => p.CategoryName)
                .ToListAsync();

            return View(pages);
        }

        [HttpGet]
        public async Task<IActionResult> View(int id)
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
    }
}