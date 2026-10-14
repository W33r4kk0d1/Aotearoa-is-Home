using Aotearoa_is_Home.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var settlementPages = await _context.SettlementPages
                .AsNoTracking()
                .Include(p => p.ContentBlocks)
                .OrderBy(p => p.CategoryName)
                .ToListAsync();

            foreach (var page in settlementPages)
            {
                page.ContentBlocks = page.ContentBlocks
                    .OrderBy(b => b.DisplayOrder)
                    .ToList();
            }

            return View(settlementPages);
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Search(string? searchString)
        {
            ViewData["SearchString"] = searchString;

            if (string.IsNullOrWhiteSpace(searchString))
            {
                return View(new List<Models.SettlementPage>());
            }

            searchString = searchString.Trim();

            var results = await _context.SettlementPages
                .AsNoTracking()
                .Include(p => p.ContentBlocks)
                .Where(p =>
                    p.CategoryName.Contains(searchString) ||
                    p.ContentBlocks.Any(c =>
                        c.Content.Contains(searchString) ||
                        c.Details.Contains(searchString)))
                .OrderBy(p => p.CategoryName)
                .ToListAsync();

            foreach (var page in results)
            {
                page.ContentBlocks = page.ContentBlocks
                    .OrderBy(c => c.DisplayOrder)
                    .ToList();
            }

            return View(results);
        }

        public IActionResult SupportingInformation()
        {
            return View();
        }
    }
}