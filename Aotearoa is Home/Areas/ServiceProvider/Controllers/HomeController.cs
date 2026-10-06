using System.Security.Claims;
using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Areas.ServiceProvider.Controllers
{
    [Area("ServiceProvider")]
    [Authorize(Roles = "Service Provider")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Get the provider profile for the logged-in user
            var providerProfile = await _context.EventProviderProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (providerProfile == null)
            {
                return NotFound("Service provider profile not found.");
            }

            // Get this provider's events
            var providerEvents = await _context.Events
                .Where(e => e.EventProviderProfileId == providerProfile.Id)
                .ToListAsync();

            // Get all approved events for the dashboard
            var allApprovedEvents = await _context.Events
                .Include(e => e.EventProviderProfile)
                .Where(e => e.Status == "Approved")
                .ToListAsync();

            // Current New Zealand time
            var newZealandTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific/Auckland");

            var currentNewZealandTime =
                TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.UtcNow,
                    newZealandTimeZone);

            // Get upcoming events
            var upcomingEvents = allApprovedEvents
                .Where(e => e.StartDate > currentNewZealandTime)
                .OrderBy(e => e.StartDate)
                .Take(3)
                .ToList();

            // Get provider's event IDs
            var eventIds = providerEvents
                .Select(e => e.Id)
                .ToList();

            // People interested in this provider's events
            var peopleInterested = await _context.EventResponses
                .CountAsync(r => eventIds.Contains(r.EventId));

            // Total views of this provider's events
            var eventViews = await _context.EventViews
                .CountAsync(v => eventIds.Contains(v.EventId));

            // Try to get the user's display name
            var providerName =
                User.FindFirstValue(ClaimTypes.GivenName)
                ?? User.Identity?.Name
                ?? "Provider";

            var viewModel = new ServiceProviderDashboardViewModel
            {
                ProviderName = providerName,

                TotalEvents = providerEvents.Count,

                UpcomingEvents = providerEvents.Count(e => e.StartDate > currentNewZealandTime),

                PendingApprovalEvents = providerEvents.Count(e => e.Status == "Pending Approval"),

                ApprovedEvents = providerEvents.Count(e => e.Status == "Approved"),

                PeopleInterested = peopleInterested,

                EventViews = eventViews,

                UpcomingEventList = upcomingEvents
            };

            return View(viewModel);
        }
    }
}