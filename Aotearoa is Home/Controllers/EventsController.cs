using Aotearoa_is_Home.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Display approved campus and community events
        [HttpGet]
        public async Task<IActionResult> Calendar(int? year, int? month)
        {
            // Use the current month when no month is selected.
            var selectedYear = year ?? DateTime.Today.Year;
            var selectedMonth = month ?? DateTime.Today.Month;

            // Make sure the selected month is valid.
            if (selectedMonth < 1)
            {
                selectedMonth = 12;
                selectedYear--;
            }
            else if (selectedMonth > 12)
            {
                selectedMonth = 1;
                selectedYear++;
            }

            // Retrieve only approved events for the selected month.
            var events = await _context.Events
                .Include(e => e.EventProviderProfile)
                .Where(e =>
                    e.Status == "Approved" &&
                    e.StartDate.Year == selectedYear &&
                    e.StartDate.Month == selectedMonth)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            // Retrieve public holidays and important reminders for the selected calendar month.
            var calendarInformation = await _context.CalendarInformations
                .Where(c =>
                    c.Date.Year == selectedYear &&
                    c.Date.Month == selectedMonth)
                .OrderBy(c => c.Date)
                .ToListAsync();

            // Pass the selected month to the calendar view.
            ViewBag.SelectedYear = selectedYear;
            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.CalendarInformation = calendarInformation;

            return View(events);

        }

        // Display details for a selected approved event
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // Find the selected event and include its service provider information.
            var eventItem = await _context.Events
                .Include(e => e.EventProviderProfile)
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.Status == "Approved");

            // If the event does not exist or has not been approved, return a Not Found page.
            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);

        }

         // Display details for a public holiday or important calendar reminder
        [HttpGet]
        public async Task<IActionResult> CalendarInformationDetails(int id)
        {
            // Find the selected calendar information.
            var information = await _context.CalendarInformations
                .FirstOrDefaultAsync(c => c.Id == id);

            // If the information does not exist, return NotFound.
            if (information == null)
            {
                return NotFound();
            }

            return View(information);

        }

    }

}