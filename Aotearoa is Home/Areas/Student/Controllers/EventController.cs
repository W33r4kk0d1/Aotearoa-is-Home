using Aotearoa_is_Home.Data;
using System.Security.Claims;
using Aotearoa_is_Home.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Aotearoa_is_Home.Areas.Student.Controllers
{
    [Area("Student")]
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Include(e => e.EventProviderProfile)
                .Where(e => e.Status == "Approved")
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return View(events);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.EventProviderProfile)
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.Status == "Approved");

            if (eventItem == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            bool isGoing = false;
            bool isFavourite = false;

            if (!string.IsNullOrEmpty(userId))
            {
                isGoing = await _context.EventResponses
                    .AnyAsync(r =>
                        r.EventId == id &&
                        r.UserId == userId);

                isFavourite = await _context.EventFavourites
                    .AnyAsync(f =>
                        f.EventId == id &&
                        f.UserId == userId);

                var alreadyViewed = await _context.EventViews
                    .AnyAsync(v =>
                        v.EventId == id &&
                        v.UserId == userId);

                if (!alreadyViewed)
                {
                    var eventView = new EventView
                    {
                        EventId = id,
                        UserId = userId,
                        ViewedAt = DateTime.UtcNow
                    };

                    _context.EventViews.Add(eventView);

                    await _context.SaveChangesAsync();
                }
            }

            ViewBag.IsGoing = isGoing;
            ViewBag.IsFavourite = isFavourite;

            ViewBag.InterestCount =
                eventItem.EnableResponses &&
                eventItem.RecordResponses
                    ? await _context.EventResponses
                        .CountAsync(r => r.EventId == id)
                    : 0;

            return View(eventItem);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Going(int id)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            if (eventItem.Status != "Approved")
            {
                return BadRequest( "This event is not currently available.");
            }

            if (eventItem.EndDate <= DateTime.Now)
            {
                TempData["EventMessage"] = "This is an old event and is no longer available for attendance or favourites.";

                return RedirectToAction(nameof(Details), new { id });
            }

            if (!eventItem.EnableResponses || !eventItem.RecordResponses)
            {
                TempData["EventMessage"] =
                    "Responses are not available for this event.";

                return RedirectToAction(nameof(Details), new { id });
            }

            // Check whether the student has already responded
            var existingResponse = await _context.EventResponses
                .FirstOrDefaultAsync(r =>
                    r.EventId == id &&
                    r.UserId == userId);

            if (existingResponse == null)
            {
                var response = new EventResponse
                {
                    EventId = id,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.EventResponses.Add(response);

                await _context.SaveChangesAsync();

                TempData["EventMessage"] = "You're marked as going to this event.";
            }
            else
            {
                TempData["EventMessage"] = "You're already marked as going.";
            }

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelGoing(int id)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var response = await _context.EventResponses
                .FirstOrDefaultAsync(r =>
                    r.EventId == id &&
                    r.UserId == userId);

            if (response != null)
            {
                _context.EventResponses.Remove(response);

                await _context.SaveChangesAsync();

                TempData["EventMessage"] = "Your attendance response has been removed.";
            }

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFavourite(int id)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            if (eventItem.EndDate <= DateTime.Now)
            {
                TempData["EventMessage"] = "This is an old event and is no longer available for attendance or favourites.";

                return RedirectToAction(nameof(Details), new { id });
            }

            var existingFavourite = await _context.EventFavourites
                .FirstOrDefaultAsync(f =>
                    f.EventId == id &&
                    f.UserId == userId);

            if (existingFavourite == null)
            {
                var favourite = new EventFavourite
                {
                    EventId = id,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.EventFavourites.Add(favourite);

                await _context.SaveChangesAsync();

                TempData["EventMessage"] = "Event saved to your favourites.";
            }
            else
            {
                TempData["EventMessage"] = "This event is already in your favourites.";
            }

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFavourite(int id)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var favourite = await _context.EventFavourites
                .FirstOrDefaultAsync(f =>
                    f.EventId == id &&
                    f.UserId == userId);

            if (favourite != null)
            {
                _context.EventFavourites.Remove(favourite);

                await _context.SaveChangesAsync();

                TempData["EventMessage"] = "Event removed from your favourites.";
            }

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Favourites()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var favourites = await _context.EventFavourites
                .Include(f => f.Event)
                .ThenInclude(e => e!.EventProviderProfile)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(favourites);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyEvents()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var events = await _context.EventResponses
                .Include(r => r.Event)
                .ThenInclude(e => e!.EventProviderProfile)
                .Where(r =>
                    r.UserId == userId &&
                    r.Event != null)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => r.Event!)
                .ToListAsync();

            return View(events);
        }
    }
}