using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Include(e => e.EventProviderProfile)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return View(events);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            string Title,
            string Description,
            string? Category,
            string Location,
            string? Region,
            DateTime StartDate,
            DateTime EndDate,
            IFormFile? image)
        {
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(Title))
            {
                ModelState.AddModelError("Title", "Title is required.");
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                ModelState.AddModelError("Description", "Description is required.");
            }

            if (string.IsNullOrWhiteSpace(Location))
            {
                ModelState.AddModelError("Location", "Location is required.");
            }

            if (EndDate <= StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after the start date.");
            }

            if (!ModelState.IsValid)
            {
                var model = new Event
                {
                    Id = eventItem.Id,
                    Title = Title,
                    Description = Description,
                    Category = Category,
                    Location = Location,
                    Region = Region,
                    StartDate = StartDate,
                    EndDate = EndDate,
                    EventProviderProfileId = eventItem.EventProviderProfileId,
                    CreatedAt = eventItem.CreatedAt,
                    ImageData = eventItem.ImageData,
                    ImageContentType = eventItem.ImageContentType
                };

                return View(model);
            }

            eventItem.Title = Title;
            eventItem.Description = Description;
            eventItem.Category = Category;
            eventItem.Location = Location;
            eventItem.Region = Region;
            eventItem.StartDate = StartDate;
            eventItem.EndDate = EndDate;

            if (image != null && image.Length > 0)
            {
                using var memoryStream = new MemoryStream();

                await image.CopyToAsync(memoryStream);

                eventItem.ImageData = memoryStream.ToArray();
                eventItem.ImageContentType = image.ContentType;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}