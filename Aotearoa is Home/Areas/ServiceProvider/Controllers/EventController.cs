using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Areas.ServiceProvider.Controllers
{
    [Area("ServiceProvider")]
    [Authorize(Roles = "Service Provider")]
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var events = _context.Events
                .Include(e => e.ServiceProvider)
                .OrderBy(e => e.StartDate)
                .ToList();

            return View(events);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Event eventItem,
            IFormFile? eventImage)
        {
            if (!ModelState.IsValid)
            {
                return View(eventItem);
            }

            if (eventImage != null && eventImage.Length > 0)
            {
                using var stream = new MemoryStream();

                await eventImage.CopyToAsync(stream);

                eventItem.ImageData = stream.ToArray();
                eventItem.ImageContentType = eventImage.ContentType;
            }

            // Temporary provider ID.
            eventItem.ServiceProviderId = 1;

            _context.Events.Add(eventItem);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}