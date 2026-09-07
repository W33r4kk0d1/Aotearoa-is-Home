using Aotearoa_is_Home.Data;
using Microsoft.AspNetCore.Mvc;
<<<<<<< HEAD
using Microsoft.EntityFrameworkCore;
=======
>>>>>>> 035b03543d471e342bc829195d9e12b5f9abcded

namespace Aotearoa_is_Home.Areas.Student.Controllers
{
    [Area("Student")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
<<<<<<< HEAD
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var pages = await _context.SettlementPages
                .OrderBy(p => p.CategoryName)
                .ToListAsync();
=======
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var pages = _context.SettlementPages.ToList();
>>>>>>> 035b03543d471e342bc829195d9e12b5f9abcded

            return View(pages);
        }
    }
}