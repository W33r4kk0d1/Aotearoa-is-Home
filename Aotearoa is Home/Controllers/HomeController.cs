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
        public IActionResult Index()
        {
            return View();
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Search(string? searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                return View(new List<Models.SettlementPage>());
            }

            searchString = searchString.Trim();

            var results = await _context.SettlementPages
                .Where(p =>
                    p.CategoryName.Contains(searchString))
                .OrderBy(p => p.CategoryName)
                .ToListAsync();

            ViewData["SearchString"] = searchString;

            return View(results);
        }
    }
}