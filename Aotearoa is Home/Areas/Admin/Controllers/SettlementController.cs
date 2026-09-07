using Aotearoa_is_Home.Data;
using Aotearoa_is_Home.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SettlementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettlementController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            SettlementPage page,
            IFormFile? backgroundImage,
            List<IFormFile>? contentImages)
        {
            if (string.IsNullOrWhiteSpace(page.CategoryName))
            {
                ModelState.AddModelError(
                    "CategoryName",
                    "Category name is required."
                );

                return View(page);
            }

            if (page.ContentBlocks == null)
            {
                page.ContentBlocks = new List<ContentBlock>();
            }

            if (backgroundImage != null && backgroundImage.Length > 0)
            {
                using var stream = new MemoryStream();

                await backgroundImage.CopyToAsync(stream);

                page.BackgroundImage = stream.ToArray();
                page.BackgroundImageContentType =
                    backgroundImage.ContentType;
            }

            var uploadedImages =
                contentImages ?? new List<IFormFile>();

            int imageIndex = 0;

            foreach (var block in page.ContentBlocks
                .OrderBy(b => b.DisplayOrder))
            {
                block.Id = 0;

                if (block.Type == "image")
                {
                    if (imageIndex < uploadedImages.Count)
                    {
                        var image = uploadedImages[imageIndex];

                        if (image != null && image.Length > 0)
                        {
                            using var stream = new MemoryStream();

                            await image.CopyToAsync(stream);

                            block.ImageData =
                                stream.ToArray();

                            block.ImageContentType =
                                image.ContentType;
                        }
                    }

                    imageIndex++;
                }
            }

            _context.SettlementPages.Add(page);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Index",
                "Home",
                new { area = "Admin" }
            );
        }

        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var page = await _context.SettlementPages
                .Include(p => p.ContentBlocks)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (page == null)
            {
                return NotFound();
            }

            page.ContentBlocks = page.ContentBlocks
                .OrderBy(b => b.DisplayOrder)
                .ToList();

            return View(page);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var page = await _context.SettlementPages
                .Include(p => p.ContentBlocks)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (page == null)
            {
                return NotFound();
            }

            page.ContentBlocks = page.ContentBlocks
                .OrderBy(b => b.DisplayOrder)
                .ToList();

            return View(page);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            SettlementPage page,
            IFormFile? backgroundImage,
            List<IFormFile>? contentImages)
        {
            var existingPage = await _context.SettlementPages
                .Include(p => p.ContentBlocks)
                .FirstOrDefaultAsync(p => p.Id == page.Id);

            if (existingPage == null)
            {
                return NotFound();
            }

            existingPage.CategoryName =
                page.CategoryName;

            if (backgroundImage != null &&
                backgroundImage.Length > 0)
            {
                using var stream = new MemoryStream();

                await backgroundImage.CopyToAsync(stream);

                existingPage.BackgroundImage =
                    stream.ToArray();

                existingPage.BackgroundImageContentType =
                    backgroundImage.ContentType;
            }

            _context.ContentBlocks.RemoveRange(
                existingPage.ContentBlocks
            );

            existingPage.ContentBlocks =
                new List<ContentBlock>();

            var uploadedImages =
                contentImages ?? new List<IFormFile>();

            int imageIndex = 0;

            if (page.ContentBlocks != null)
            {
                foreach (var block in page.ContentBlocks
                    .OrderBy(b => b.DisplayOrder))
                {
                    block.Id = 0;
                    block.SettlementPageId =
                        existingPage.Id;

                    if (block.Type == "image")
                    {
                        if (imageIndex < uploadedImages.Count)
                        {
                            var image =
                                uploadedImages[imageIndex];

                            if (image != null &&
                                image.Length > 0)
                            {
                                using var stream =
                                    new MemoryStream();

                                await image.CopyToAsync(
                                    stream
                                );

                                block.ImageData =
                                    stream.ToArray();

                                block.ImageContentType =
                                    image.ContentType;
                            }
                        }

                        imageIndex++;
                    }

                    existingPage.ContentBlocks.Add(block);
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Index",
                "Home",
                new { area = "Admin" }
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var page = await _context.SettlementPages
                .Include(p => p.ContentBlocks)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (page == null)
            {
                return NotFound();
            }

            _context.ContentBlocks.RemoveRange(
                page.ContentBlocks
            );

            _context.SettlementPages.Remove(page);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Index",
                "Home",
                new { area = "Admin" }
            );
        }

        [HttpGet]
        public async Task<IActionResult> BackgroundImage(int id)
        {
            var page = await _context.SettlementPages
                .FirstOrDefaultAsync(p => p.Id == id);

            if (page == null ||
                page.BackgroundImage == null)
            {
                return NotFound();
            }

            return File(
                page.BackgroundImage,
                page.BackgroundImageContentType ??
                "image/jpeg"
            );
        }

        [HttpGet]
        public async Task<IActionResult> ContentImage(int id)
        {
            var block = await _context.ContentBlocks
                .FirstOrDefaultAsync(b => b.Id == id);

            if (block == null ||
                block.ImageData == null)
            {
                return NotFound();
            }

            return File(
                block.ImageData,
                block.ImageContentType ??
                "image/jpeg"
            );
        }
    }
}
