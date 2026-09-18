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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var events = await _context.Events
                .Include(e => e.EventProviderProfile)
                .Where(e => e.EventProviderProfile!.UserId == userId)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Content("ERROR: Could not find logged-in user.");
            }

            var provider = await _context.EventProviderProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (provider == null)
            {
                return Content(
                    "ERROR: No EventProviderProfile found for this user. User ID: "
                    + userId);
            }

            eventItem.EventProviderProfileId = provider.Id;

            ModelState.Remove(nameof(Event.EventProviderProfileId));

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

                return Content(
                    "VALIDATION ERROR:\n" +
                    string.Join("\n", errors));
            }

            if (eventImage != null && eventImage.Length > 0)
            {
                using var stream = new MemoryStream();

                await eventImage.CopyToAsync(stream);

                eventItem.ImageData = stream.ToArray();
                eventItem.ImageContentType = eventImage.ContentType;
            }

            try
            {
                _context.Events.Add(eventItem);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Content(
                    "DATABASE ERROR:\n\n" +
                    ex.InnerException?.Message ??
                    ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}