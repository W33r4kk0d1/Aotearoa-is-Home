using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var settlementPages = await _context.SettlementPages
                .OrderBy(p => p.CategoryName)
                .ToListAsync();

            var events = await _context.Events
                .Include(e => e.EventProviderProfile)
                .Where(e => e.Status == "Approved")
                .OrderBy(e => e.StartDate)
                .Take(3)
                .ToListAsync();

            var model = new StudentHomeViewModel
            {
                SettlementPages = settlementPages,
                Events = events
            };

            return View(model);
        }
        
    }
}