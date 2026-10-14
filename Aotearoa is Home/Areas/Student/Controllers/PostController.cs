using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aotearoa_is_Home.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PostController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var posts = await _context.CommunityPosts
                .AsNoTracking()
                .Where(p => p.Status == "Approved")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var userId = _userManager.GetUserId(User);

            var savedIds = await _context.CommunityPostFavourites
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .Select(f => f.CommunityPostId)
                .ToListAsync();

            ViewBag.SavedPostIds = savedIds;

            return View(posts);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> Create(
            string? Title,
            string? Category,
            string? Description,
            IFormFile? Image)
        {
            var allowedCategories = new[]
            {
                "Accommodation",
                "Education",
                "Employment",
                "Healthcare",
                "General"
            };

            if (string.IsNullOrWhiteSpace(Title) ||
                string.IsNullOrWhiteSpace(Category) ||
                string.IsNullOrWhiteSpace(Description))
            {
                ModelState.AddModelError(
                    "",
                    "Please complete all required fields.");

                return View();
            }

            if (Title.Length > 150 ||
                Description.Length > 5000 ||
                !allowedCategories.Contains(Category))
            {
                ModelState.AddModelError(
                    "",
                    "Please check your post details and category.");

                return View();
            }

            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var post = new CommunityPost
            {
                Title = Title.Trim(),
                Category = Category,
                Description = Description.Trim(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending"
            };

            if (Image != null && Image.Length > 0)
            {
                const long maxSize = 5 * 1024 * 1024;

                var allowedTypes = new[]
                {
                    "image/jpeg",
                    "image/png",
                    "image/webp"
                };

                if (Image.Length > maxSize ||
                    !allowedTypes.Contains(Image.ContentType))
                {
                    ModelState.AddModelError(
                        "",
                        "Choose a JPG, PNG or WebP image under 5 MB.");

                    return View();
                }

                using var stream = new MemoryStream();
                await Image.CopyToAsync(stream);
                var bytes = stream.ToArray();

                bool validImage =
                    (Image.ContentType == "image/jpeg" &&
                     bytes.Length >= 3 &&
                     bytes[0] == 0xFF &&
                     bytes[1] == 0xD8 &&
                     bytes[2] == 0xFF) ||

                    (Image.ContentType == "image/png" &&
                     bytes.Length >= 8 &&
                     bytes.Take(8).SequenceEqual(
                         new byte[] {
                             137, 80, 78, 71,
                             13, 10, 26, 10
                         })) ||

                    (Image.ContentType == "image/webp" &&
                     bytes.Length >= 12 &&
                     System.Text.Encoding.ASCII.GetString(
                         bytes, 0, 4) == "RIFF" &&
                     System.Text.Encoding.ASCII.GetString(
                         bytes, 8, 4) == "WEBP");

                if (!validImage)
                {
                    ModelState.AddModelError(
                        "",
                        "The image file is not valid.");

                    return View();
                }

                post.ImageData = bytes;
                post.ImageContentType = Image.ContentType;
            }

            _context.CommunityPosts.Add(post);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Your post was submitted and is awaiting admin approval.";

            return RedirectToAction(nameof(MyPosts));
        }

        [HttpGet]
        public async Task<IActionResult> MyPosts()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var posts = await _context.CommunityPosts
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavourite(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var postExists = await _context.CommunityPosts
                .AnyAsync(p => p.Id == id && p.Status == "Approved");

            if (!postExists)
                return NotFound();

            var favourite = await _context.CommunityPostFavourites
                .FirstOrDefaultAsync(f =>
                    f.CommunityPostId == id &&
                    f.UserId == userId);

            bool isSaved;

            if (favourite == null)
            {
                _context.CommunityPostFavourites.Add(
                    new CommunityPostFavourite
                    {
                        CommunityPostId = id,
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow
                    });

                isSaved = true;
            }
            else
            {
                _context.CommunityPostFavourites.Remove(favourite);
                isSaved = false;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, isSaved });
        }

        [HttpGet]
        public async Task<IActionResult> Saved()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var posts = await _context.CommunityPostFavourites
                .AsNoTracking()
                .Where(f => f.UserId == userId &&
                            f.CommunityPost != null &&
                            f.CommunityPost.Status == "Approved")
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => f.CommunityPost!)
                .ToListAsync();

            ViewBag.SavedPostIds = posts.Select(p => p.Id).ToList();

            return View(posts);
        }

    }
}
