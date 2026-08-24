using Aotearoa_is_Home.Data;
using Microsoft.AspNetCore.Mvc;

namespace Aotearoa_is_Home.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var pages = _context.SettlementPages.ToList();

            return View(pages);
        }
    }
}