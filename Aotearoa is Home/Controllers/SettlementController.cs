using Aotearoa_is_Home.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Controllers
{
    public class SettlementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettlementController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var page = await _context.SettlementPages
                .AsNoTracking()
                .Include(p => p.ContentBlocks)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (page == null)
                return NotFound();

            page.ContentBlocks = page.ContentBlocks
                .OrderBy(b => b.DisplayOrder)
                .ToList();

            return View("~/Areas/Student/Views/Settlement/View.cshtml", page);
        }
    }
}