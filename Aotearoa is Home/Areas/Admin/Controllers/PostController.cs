
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

        // ADMIN - MANAGE ALL POSTS
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var posts = await _context.CommunityPosts
                .AsNoTracking()
                .OrderBy(p => p.Status == "Pending" ? 0 : 1)
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync();

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
