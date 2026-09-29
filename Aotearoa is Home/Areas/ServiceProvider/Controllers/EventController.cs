using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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


        // ## Display Service Provider events
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var events = await _context.Events
                .Include(e => e.EventProviderProfile)
                .Where(e =>
                    e.EventProviderProfile!.UserId == userId)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return View(events);
        }


        // ## Display Create Event page
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // ## Create a new event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Event eventItem,
            IFormFile? eventImage)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Content(
                    "ERROR: Could not find logged-in user.");
            }


            // ## Find Service Provider profile
            var provider =
                await _context.EventProviderProfiles
                    .FirstOrDefaultAsync(
                        p => p.UserId == userId);

            if (provider == null)
            {
                return Content(
                    "ERROR: No EventProviderProfile found " +
                    "for this user. User ID: " +
                    userId);
            }


            // ## Connect event to Service Provider
            eventItem.EventProviderProfileId =
                provider.Id;


            // ## Remove automatically assigned field
            // ## from model validation
            ModelState.Remove(
                nameof(Event.EventProviderProfileId));


            // ## Validate Region
            if (string.IsNullOrWhiteSpace(
                eventItem.Region))
            {
                ModelState.AddModelError(
                    nameof(Event.Region),
                    "Please select a region.");
            }


            // ## Validate Category
            if (string.IsNullOrWhiteSpace(
                eventItem.Category))
            {
                ModelState.AddModelError(
                    nameof(Event.Category),
                    "Please select an event category.");
            }


            // ## Return validation errors
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .Where(x =>
                        !string.IsNullOrEmpty(x))
                    .ToList();

                return Content(
                    "VALIDATION ERROR:\n" +
                    string.Join("\n", errors));
            }


            // ## Save uploaded event image
            if (eventImage != null &&
                eventImage.Length > 0)
            {
                using var stream =
                    new MemoryStream();

                await eventImage
                    .CopyToAsync(stream);

                eventItem.ImageData =
                    stream.ToArray();

                eventItem.ImageContentType =
                    eventImage.ContentType;
            }


            // ## Set event creation time
            eventItem.CreatedAt =
                DateTime.UtcNow;


            // ## Save event
            try
            {
                _context.Events.Add(eventItem);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Content(
                    "DATABASE ERROR:\n\n" +
                    (ex.InnerException?.Message ??
                     ex.Message));
            }


            return RedirectToAction(
                nameof(Index));
        }


        // ## Display Edit Event page
        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }


            // ## Find event
            // ## Event must belong to logged-in provider
            var eventItem =
                await _context.Events
                    .Include(e =>
                        e.EventProviderProfile)
                    .FirstOrDefaultAsync(e =>
                        e.Id == id &&
                        e.EventProviderProfile!
                            .UserId == userId);


            // ## Event not found
            if (eventItem == null)
            {
                return NotFound();
            }


            return View(eventItem);
        }


        // ## Update Event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Event eventItem,
            IFormFile? eventImage)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }


            // ## Find existing event
            // ## and verify event ownership
            var existingEvent =
                await _context.Events
                    .Include(e =>
                        e.EventProviderProfile)
                    .FirstOrDefaultAsync(e =>
                        e.Id == id &&
                        e.EventProviderProfile!
                            .UserId == userId);


            // ## Event does not exist
            // ## or does not belong to this provider
            if (existingEvent == null)
            {
                return NotFound();
            }


            // ## Provider ID does not come
            // ## from the Edit form
            ModelState.Remove(
                nameof(Event.EventProviderProfileId));


            // ## Image information is preserved
            // ## from the existing database record
            ModelState.Remove(
                nameof(Event.ImageData));

            ModelState.Remove(
                nameof(Event.ImageContentType));


            // ## Validate Region
            if (string.IsNullOrWhiteSpace(
                eventItem.Region))
            {
                ModelState.AddModelError(
                    nameof(Event.Region),
                    "Please select a region.");
            }


            // ## Validate Category
            if (string.IsNullOrWhiteSpace(
                eventItem.Category))
            {
                ModelState.AddModelError(
                    nameof(Event.Category),
                    "Please select an event category.");
            }


            // ## Return Edit page when validation fails
            if (!ModelState.IsValid)
            {
                // ## Keep database values needed
                // ## by the Edit view
                eventItem.Id =
                    existingEvent.Id;

                eventItem.EventProviderProfileId =
                    existingEvent
                        .EventProviderProfileId;

                eventItem.ImageData =
                    existingEvent.ImageData;

                eventItem.ImageContentType =
                    existingEvent
                        .ImageContentType;

                eventItem.CreatedAt =
                    existingEvent.CreatedAt;

                return View(eventItem);
            }


            // ## Update editable event information
            existingEvent.Title =
                eventItem.Title;

            existingEvent.Description =
                eventItem.Description;

            existingEvent.Location =
                eventItem.Location;

            existingEvent.Region =
                eventItem.Region;

            existingEvent.StartDate =
                eventItem.StartDate;

            existingEvent.EndDate =
                eventItem.EndDate;

            existingEvent.Category =
                eventItem.Category;


            // ## Replace image only when
            // ## a new image is uploaded
            if (eventImage != null &&
                eventImage.Length > 0)
            {
                using var stream =
                    new MemoryStream();

                await eventImage
                    .CopyToAsync(stream);

                existingEvent.ImageData =
                    stream.ToArray();

                existingEvent.ImageContentType =
                    eventImage.ContentType;
            }


            // ## Save updated event
            try
            {
                await _context
                    .SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Content(
                    "DATABASE ERROR:\n\n" +
                    (ex.InnerException?.Message ??
                     ex.Message));
            }


            return RedirectToAction(
                nameof(Index));
        }
    }
}