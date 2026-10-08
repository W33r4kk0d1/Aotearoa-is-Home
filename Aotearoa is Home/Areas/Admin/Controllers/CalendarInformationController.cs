using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Super Admin")]
    public class CalendarInformationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CalendarInformationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var information = await _context.CalendarInformations
                .OrderBy(c => c.Date)
                .ThenBy(c => c.Title)
                .ToListAsync();

            return View(information);
        }

        // CREATE
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CalendarInformation information)
        {
            if (!ModelState.IsValid)
            {
                return View(information);
            }

            _context.CalendarInformations.Add(information);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // EDIT
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var information = await _context.CalendarInformations
                .FindAsync(id);

            if (information == null)
            {
                return NotFound();
            }

            return View(information);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            CalendarInformation information)
        {
            if (id != information.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(information);
            }

            try
            {
                _context.Update(information);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CalendarInformationExists(information.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }


        // DELETE
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var information = await _context.CalendarInformations
                .FindAsync(id);

            if (information == null)
            {
                return NotFound();
            }

            return View(information);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var information = await _context.CalendarInformations
                .FindAsync(id);

            if (information != null)
            {
                _context.CalendarInformations.Remove(information);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        // CHECK WHETHER A RECORD EXISTS
        private bool CalendarInformationExists(int id)
        {
            return _context.CalendarInformations
                .Any(e => e.Id == id);
        }

    }
}