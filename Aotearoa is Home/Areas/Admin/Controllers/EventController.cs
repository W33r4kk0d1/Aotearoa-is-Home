using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Super Admin")]
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        private const int PageSize = 10;

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchString,
            string? status,
            string? category,
            int page = 1)
        {
            var query = _context.Events
                .AsNoTracking()
                .Include(e => e.EventProviderProfile)
                .AsQueryable();

            // Search by title, description, or location
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                query = query.Where(e =>
                    e.Title.Contains(searchString) ||
                    e.Description.Contains(searchString) ||
                    e.Location.Contains(searchString));
            }

            // Filter by status
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(e => e.Status == status);
            }

            // Filter by category
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(e => e.Category == category);
            }

            // Count events matching the filters
            var totalEvents = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(
                totalEvents / (double)PageSize);

            // Keep the requested page within valid bounds
            page = Math.Max(1, Math.Min(page, Math.Max(totalPages, 1)));

            // Get available categories
            var categories = await _context.Events
                .AsNoTracking()
                .Where(e => e.Category != null && e.Category != "")
                .Select(e => e.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            // Get only the events for the current page
            var events = await query
                .OrderByDescending(e => e.CreatedAt)
                .ThenByDescending(e => e.Id)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Count registered attendees for events on this page
            var eventIds = events.Select(e => e.Id).ToList();

            var attendeeCounts = await _context.EventResponses
                .AsNoTracking()
                .Where(r => eventIds.Contains(r.EventId))
                .GroupBy(r => r.EventId)
                .Select(g => new
                {
                    EventId = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.EventId, x => x.Count);

            // Preserve search and filter values
            ViewBag.SearchString = searchString;
            ViewBag.Status = status;
            ViewBag.Category = category;
            ViewBag.Categories = categories;

            // Pagination information
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalEvents = totalEvents;
            ViewBag.PageSize = PageSize;

            // Attendee counts for the current page
            ViewBag.AttendeeCounts = attendeeCounts;

            return View(events);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var eventItem = await _context.Events
                .AsNoTracking()
                .Include(e => e.EventProviderProfile)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
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

    
        [HttpPost]
        [Authorize(Roles = "Super Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            if (eventItem.Status != "Pending Approval")
            {
                TempData["ErrorMessage"] =
                    "Only events awaiting approval can be approved.";

                return RedirectToAction(nameof(Index));
            }

            eventItem.Status = "Approved";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Event approved successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            // Can also reject an event that was previously approved.
            eventItem.Status = "Rejected";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Event rejected successfully and removed from approved status.";

            return RedirectToAction(nameof(Index));
        }

    }
}