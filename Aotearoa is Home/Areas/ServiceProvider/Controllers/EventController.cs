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

        // VIEW EVENT
        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var eventItem = await _context.Events
                .Include(e => e.EventProviderProfile)
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.EventProviderProfile!.UserId == userId);

            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }


        // ## Display Service Provider events
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? category,
            string? region,
            string? status,
            string? dateFilter)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Get all events belonging to the logged-in provider
            var query = _context.Events
                .Include(e => e.EventProviderProfile)
                .Where(e => e.EventProviderProfile!.UserId == userId)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(e =>
                    e.Title.Contains(search) ||
                    e.Location.Contains(search) ||
                    e.Description.Contains(search));
            }

            // Category filter
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(e => e.Category == category);
            }

            // Region filter
            if (!string.IsNullOrWhiteSpace(region))
            {
                query = query.Where(e => e.Region == region);
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(e => e.Status == status);
            }

            // Date filter
            var newZealandTimeZone =
                TimeZoneInfo.FindSystemTimeZoneById("Pacific/Auckland");

            var currentNewZealandTime =
                TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.UtcNow,
                    newZealandTimeZone);

            if (dateFilter == "upcoming")
            {
                query = query.Where(e =>
                    e.StartDate > currentNewZealandTime);
            }
            else if (dateFilter == "past")
            {
                query = query.Where(e =>
                    e.StartDate <= currentNewZealandTime);
            }

            // Get filtered events
            var events = await query
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            // Keep filter options available even after filtering
            var allProviderEvents = await _context.Events
                .Where(e => e.EventProviderProfile!.UserId == userId)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedRegion = region;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedDateFilter = dateFilter;

            ViewBag.Categories = allProviderEvents
                .Where(e => !string.IsNullOrWhiteSpace(e.Category))
                .Select(e => e.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            ViewBag.Regions = allProviderEvents
                .Where(e => !string.IsNullOrWhiteSpace(e.Region))
                .Select(e => e.Region!)
                .Distinct()
                .OrderBy(r => r)
                .ToList();

            ViewBag.Statuses = allProviderEvents
                .Where(e => !string.IsNullOrWhiteSpace(e.Status))
                .Select(e => e.Status)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

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
            // GET LOGGED-IN USER
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);


            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }


            // FIND SERVICE PROVIDER PROFILE

            var provider =
                await _context.EventProviderProfiles
                    .FirstOrDefaultAsync(
                        p => p.UserId == userId);


            if (provider == null)
            {
                return Content(
                    "ERROR: No EventProviderProfile found " +
                    "for this user."
                );
            }


            // CONNECT EVENT TO PROVIDER

            eventItem.EventProviderProfileId =
                provider.Id;

            ModelState.Remove(
                nameof(Event.EventProviderProfileId)
            );


            // NORMALISE USER INPUT

            eventItem.Title =
                eventItem.Title?.Trim() ?? string.Empty;

            eventItem.Description =
                eventItem.Description?.Trim() ?? string.Empty;

            eventItem.Location =
                eventItem.Location?.Trim() ?? string.Empty;

            eventItem.OfficialEventLink =
                string.IsNullOrWhiteSpace(
                    eventItem.OfficialEventLink)
                    ? null
                    : eventItem.OfficialEventLink.Trim();


            // REGION VALIDATION

            if (string.IsNullOrWhiteSpace(eventItem.Region))
            {
                ModelState.AddModelError(
                    nameof(Event.Region),
                    "Please select a region."
                );
            }


            // CATEGORY VALIDATION

            if (string.IsNullOrWhiteSpace(eventItem.Category))
            {
                ModelState.AddModelError(
                    nameof(Event.Category),
                    "Please select an event category."
                );
            }


            // RESPONSE SETTINGS

            if (!eventItem.EnableResponses)
            {
                eventItem.RecordResponses = false;
                eventItem.ShowResponseCount = false;
                eventItem.EmailOnResponse = false;
            }

            if (eventItem.ShowResponseCount &&
                !eventItem.RecordResponses)
            {
                ModelState.AddModelError(
                    nameof(Event.ShowResponseCount),
                    "Interest count cannot be displayed unless responses are recorded."
                );
            }


            // DUPLICATE EVENT CHECK

            if (!string.IsNullOrWhiteSpace(eventItem.Title) &&
                eventItem.StartDate != default)
            {
                var normalisedTitle =
                    eventItem.Title
                        .Trim()
                        .ToLower();

                var eventDate =
                    eventItem.StartDate.Date;


                var duplicateExists =
                    await _context.Events.AnyAsync(e =>
                        e.EventProviderProfileId == provider.Id &&
                        e.Title.Trim().ToLower() == normalisedTitle &&
                        e.StartDate.Date == eventDate
                    );


                if (duplicateExists)
                {
                    ModelState.AddModelError(
                        nameof(Event.Title),
                        "You have already submitted an event with the same title on this date."
                    );
                }
            }


            // VALIDATE MODEL

            if (!ModelState.IsValid)
            {
                return View(eventItem);
            }


            // SAVE EVENT IMAGE

            if (eventImage != null &&
                eventImage.Length > 0)
            {
                using var stream =
                    new MemoryStream();

                await eventImage.CopyToAsync(stream);

                eventItem.ImageData =
                    stream.ToArray();

                eventItem.ImageContentType =
                    eventImage.ContentType;
            }


            // SET EVENT INFORMATION

            eventItem.CreatedAt =
                DateTime.UtcNow;


            eventItem.Status =
                "Pending Approval";


            // SAVE EVENT

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
                    ex.Message)
                );
            }

            TempData["SuccessMessage"] = "Event successfully created.";

            // RETURN TO MANAGE EVENTS

            return RedirectToAction(
                nameof(Index)
            );
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
            // GET LOGGED-IN USER
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }


            // FIND EXISTING EVENT AND VERIFY EVENT OWNERSHIP
            var existingEvent =
                await _context.Events
                    .Include(e => e.EventProviderProfile)
                    .FirstOrDefaultAsync(e =>
                        e.Id == id &&
                        e.EventProviderProfile!
                            .UserId == userId);


            // EVENT DOES NOT EXIST OR DOES NOT BELONG TO THIS PROVIDER
            if (existingEvent == null)
            {
                return NotFound();
            }


            // PROVIDER ID DOES NOT COME FROM THE EDIT FORM
            ModelState.Remove( nameof(Event.EventProviderProfileId));
            // IMAGE INFORMATION IS PRESERVED
            ModelState.Remove( nameof(Event.ImageData));
            ModelState.Remove( nameof(Event.ImageContentType));
            // NORMALISE USER INPUT
            eventItem.Title =
                eventItem.Title?.Trim()
                ?? string.Empty;

            eventItem.Description =
                eventItem.Description?.Trim()
                ?? string.Empty;

            eventItem.Location =
                eventItem.Location?.Trim()
                ?? string.Empty;

            eventItem.OfficialEventLink =
                string.IsNullOrWhiteSpace(
                    eventItem.OfficialEventLink)
                    ? null
                    : eventItem.OfficialEventLink.Trim();


            // REGION VALIDATION

            if (string.IsNullOrWhiteSpace(
                eventItem.Region))
            {
                ModelState.AddModelError(
                    nameof(Event.Region),
                    "Please select a region."
                );
            }


            // CATEGORY VALIDATION

            if (string.IsNullOrWhiteSpace(
                eventItem.Category))
            {
                ModelState.AddModelError(
                    nameof(Event.Category),
                    "Please select an event category."
                );
            }


            // DATE VALIDATION

            var newZealandTimeZone =
                TimeZoneInfo.FindSystemTimeZoneById("Pacific/Auckland");

            var currentNewZealandTime =
                TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.UtcNow,
                    newZealandTimeZone);

            if (eventItem.StartDate == default)
            {
                ModelState.AddModelError(
                    nameof(Event.StartDate),
                    "Please select a start date and time."
                );
            }
            else if (eventItem.StartDate <= currentNewZealandTime)
            {
                ModelState.AddModelError(
                    nameof(Event.StartDate),
                    "Event start date and time must be in the future."
                );
            }


            if (eventItem.EndDate == default)
            {
                ModelState.AddModelError(
                    nameof(Event.EndDate),
                    "Please select an end date and time."
                );
            }
            else if (
                eventItem.StartDate != default &&
                eventItem.EndDate <= eventItem.StartDate)
            {
                ModelState.AddModelError(
                    nameof(Event.EndDate),
                    "Event end date and time must be after the start date and time."
                );
            }

            // RESPONSE SETTINGS

            if (!eventItem.EnableResponses)
            {
                eventItem.RecordResponses = false;
                eventItem.ShowResponseCount = false;
                eventItem.EmailOnResponse = false;
            }


            if (
                eventItem.ShowResponseCount &&
                !eventItem.RecordResponses)
            {
                ModelState.AddModelError(
                    nameof(Event.ShowResponseCount),
                    "Interest count cannot be displayed unless responses are recorded."
                );
            }


            // DUPLICATE EVENT CHECK

            if (
                !string.IsNullOrWhiteSpace(eventItem.Title) &&
                eventItem.StartDate != default)
            {
                var normalisedTitle =
                    eventItem.Title
                        .Trim()
                        .ToLower();

                var eventDate =
                    eventItem.StartDate.Date;


                var duplicateExists =
                    await _context.Events.AnyAsync(e =>
                        e.Id != existingEvent.Id &&
                        e.EventProviderProfileId ==
                            existingEvent.EventProviderProfileId &&
                        e.Title.Trim().ToLower() ==
                            normalisedTitle &&
                        e.StartDate.Date ==
                            eventDate
                    );


                if (duplicateExists)
                {
                    ModelState.AddModelError(
                        nameof(Event.Title),
                        "You have already submitted an event with the same title on this date."
                    );
                }
            }


            // RETURN TO EDIT PAGE WHEN VALIDATION FAILS

            if (!ModelState.IsValid)
            {
                eventItem.Id = existingEvent.Id;
                eventItem.EventProviderProfileId = existingEvent.EventProviderProfileId;
                eventItem.ImageData = existingEvent.ImageData;
                eventItem.ImageContentType = existingEvent.ImageContentType;
                eventItem.CreatedAt = existingEvent.CreatedAt;
                return View(eventItem);
            }


            // UPDATE EDITABLE EVENT INFORMATION

            existingEvent.Title = eventItem.Title;

            existingEvent.Description = eventItem.Description;

            existingEvent.Location = eventItem.Location;

            existingEvent.Region = eventItem.Region;

            existingEvent.StartDate = eventItem.StartDate;

            existingEvent.EndDate = eventItem.EndDate;

            existingEvent.Category = eventItem.Category;

            existingEvent.OfficialEventLink = eventItem.OfficialEventLink;

            existingEvent.EnableResponses = eventItem.EnableResponses;

            existingEvent.RecordResponses = eventItem.RecordResponses;

            existingEvent.ShowResponseCount = eventItem.ShowResponseCount;

            existingEvent.EmailOnResponse = eventItem.EmailOnResponse;


            // REPLACE IMAGE ONLY WHEN A NEW IMAGE IS UPLOADED

            if (
                eventImage != null &&
                eventImage.Length > 0)
            {
                using var stream =
                    new MemoryStream();

                await eventImage.CopyToAsync(stream);

                existingEvent.ImageData =
                    stream.ToArray();

                existingEvent.ImageContentType =
                    eventImage.ContentType;
            }


            // SAVE UPDATED EVENT

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Content(
                    "DATABASE ERROR:\n\n" +
                    (ex.InnerException?.Message ??
                    ex.Message)
                );
            }

            TempData["SuccessMessage"] = "Event successfully updated.";

            // RETURN TO MANAGE EVENTS

            return RedirectToAction(
                nameof(Index)
            );
        }

        // DELETE EVENT

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }


            // Find event and verify ownership
            var eventItem =
                await _context.Events
                    .Include(e =>
                        e.EventProviderProfile)
                    .FirstOrDefaultAsync(e =>
                        e.Id == id &&
                        e.EventProviderProfile!
                            .UserId == userId);


            // Event does not exist
            // or does not belong to this provider
            if (eventItem == null)
            {
                return NotFound();
            }


            // Delete event
            _context.Events.Remove(eventItem);

            await _context.SaveChangesAsync();


            // Success message
            TempData["SuccessMessage"] = "Event successfully deleted.";


            return RedirectToAction(nameof(Index));
        }
    }
}