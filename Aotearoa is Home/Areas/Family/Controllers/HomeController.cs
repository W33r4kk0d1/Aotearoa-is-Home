using Aotearoa_is_Home.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Areas.Family.Controllers
{
    [Area("Family")]
    [Authorize(Roles = "Family Member")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var pages = await _context.SettlementPages
                .OrderBy(p => p.CategoryName)
                .ToListAsync();

            return View(pages);
        }
    }
}