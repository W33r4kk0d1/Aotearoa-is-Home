
using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Super Admin")]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        private const int PageSize = 10;

        // ADMIN - MANAGE ALL POSTS
        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchString,
            string? status,
            string? category,
            int page = 1)
        {
            // Start with all community posts
            var query = _context.CommunityPosts
                .AsNoTracking()
                .AsQueryable();

            // Search by title and description
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                query = query.Where(p =>
                    p.Title.Contains(searchString) ||
                    p.Description.Contains(searchString));
            }

            // Filter by status
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.Status == status);
            }

            // Filter by category
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category == category);
            }

            // Get the total number of matching posts
            var totalPosts = await query.CountAsync();

            // Calculate pagination
            var totalPages = (int)Math.Ceiling(
                totalPosts / (double)PageSize);

            page = Math.Max(1, Math.Min(page,
                Math.Max(totalPages, 1)));

            // Get the categories for the filter dropdown
            var categories = await _context.CommunityPosts
                .AsNoTracking()
                .Where(p => p.Category != null &&
                            p.Category != "")
                .Select(p => p.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            // Newest posts first, with only 10 posts per page
            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Send pagination and filter information to the view
            ViewBag.SearchString = searchString;
            ViewBag.Status = status;
            ViewBag.Category = category;
            ViewBag.Categories = categories;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalPosts = totalPosts;
            ViewBag.PageSize = PageSize;

            // Dashboard counts - across all posts
            ViewBag.PendingCount = await _context.CommunityPosts
                .CountAsync(p => p.Status == "Pending");

            ViewBag.ApprovedCount = await _context.CommunityPosts
                .CountAsync(p => p.Status == "Approved");

            ViewBag.RejectedCount = await _context.CommunityPosts
                .CountAsync(p => p.Status == "Rejected");

            return View(posts);
        }


        // ADMIN - APPROVE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var post = await _context.CommunityPosts
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            if (post.Status != "Pending")
            {
                TempData["ErrorMessage"] =
                    "Only pending posts can be approved.";

                return RedirectToAction(nameof(Index));
            }

            post.Status = "Approved";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Post approved and published successfully.";

            return RedirectToAction(nameof(Index));
        }

        // ADMIN - REJECT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var post = await _context.CommunityPosts
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            if (post.Status != "Pending")
            {
                TempData["ErrorMessage"] =
                    "Only pending posts can be rejected.";

                return RedirectToAction(nameof(Index));
            }

            post.Status = "Rejected";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Post rejected successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
