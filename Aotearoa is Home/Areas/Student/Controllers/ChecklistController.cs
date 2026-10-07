using Microsoft.AspNetCore.Mvc;
using Aotearoa_is_Home.Models;
using Aotearoa_is_Home.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Aotearoa_is_Home.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class ChecklistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChecklistController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // ============================================================
        // MY CHECKLISTS
        // ============================================================

        // GET: /Student/Checklist
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var checklists = await _context.StudentChecklists
                .Where(c => c.UserId == userId)
                .Include(c => c.Items)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(checklists);
        }


        // ============================================================
        // CREATE CHECKLIST - DISPLAY PAGE
        // ============================================================

        // GET: /Student/Checklist/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // ============================================================
        // CREATE CHECKLIST - SAVE
        // ============================================================

        // POST: /Student/Checklist/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string name,
            List<string>? selectedTopics)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // --------------------------------------------------------
            // Validate checklist name
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError(
                    "name",
                    "Please enter a name for your checklist.");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            // --------------------------------------------------------
            // Create checklist journey
            // --------------------------------------------------------

            var checklist = new StudentChecklist
            {
                UserId = userId,
                Name = name.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsArchived = false
            };

            _context.StudentChecklists.Add(checklist);

            await _context.SaveChangesAsync();


            // --------------------------------------------------------
            // Create selected checklist items
            // --------------------------------------------------------

            if (selectedTopics != null && selectedTopics.Any())
            {
                var checklistItems = GetChecklistTopics()
                    .Where(topic => selectedTopics.Contains(topic.Title))
                    .Select(topic => new ChecklistItem
                    {
                        UserId = userId,
                        StudentChecklistId = checklist.Id,
                        Category = topic.Category,
                        Title = topic.Title,
                        Description = topic.Description,
                        IsCompleted = false
                    })
                    .ToList();

                if (checklistItems.Any())
                {
                    _context.ChecklistItems.AddRange(checklistItems);

                    await _context.SaveChangesAsync();
                }
            }


            // Open the newly created checklist

            return RedirectToAction(
                            "ViewChecklist",
                            "Checklist",
                            new
                            {
                                area = "Student",
                                id = checklist.Id
                            });
        }


        // GET: /Student/Checklist/View/1
        [HttpGet]
        public async Task<IActionResult> ViewChecklist(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var checklist = await _context.StudentChecklists
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    c.UserId == userId);

            if (checklist == null)
            {
                return NotFound();
            }

            // --------------------------------------------------------
            // Load settlement information for checklist topics
            // --------------------------------------------------------

            var settlementPages = await _context.SettlementPages
                .Include(p => p.ContentBlocks)
                .ToListAsync();

            ViewBag.SettlementPages = settlementPages;

            return View("View", checklist);
        }

        // UPDATE CHECKLIST PROGRESS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            int checklistId,
            List<int>? completedItems)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // --------------------------------------------------------
            // Make sure this checklist belongs to the logged-in
            // student
            // --------------------------------------------------------

            var checklist = await _context.StudentChecklists
                .FirstOrDefaultAsync(c =>
                    c.Id == checklistId &&
                    c.UserId == userId);

            if (checklist == null)
            {
                return NotFound();
            }

            // --------------------------------------------------------
            // Get only items belonging to this checklist
            // --------------------------------------------------------

            var checklistItems = await _context.ChecklistItems
                .Where(c =>
                    c.StudentChecklistId == checklistId &&
                    c.UserId == userId)
                .ToListAsync();

            completedItems ??= new List<int>();

            // --------------------------------------------------------
            // Update completion status
            // --------------------------------------------------------

            foreach (var item in checklistItems)
            {
                item.IsCompleted = completedItems.Contains(item.Id);
            }

            await _context.SaveChangesAsync();

            // --------------------------------------------------------
            // Return to checklist
            // --------------------------------------------------------

            return RedirectToAction(
                "ViewChecklist",
                "Checklist",
                new
                {
                    area = "Student",
                    id = checklistId
                });
        }

            

        // GET: /Student/Checklist/Customize/1
        [HttpGet]
        public async Task<IActionResult> Customize(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var checklist = await _context.StudentChecklists
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    c.UserId == userId);

            if (checklist == null)
            {
                return NotFound();
            }

            return View(checklist);
        }


        // ============================================================
        // CUSTOMIZE CHECKLIST - SAVE CHANGES
        // ============================================================

        // POST: /Student/Checklist/Customize/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Customize(
            int id,
            string name,
            List<string>? selectedTopics)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // --------------------------------------------------------
            // Find checklist belonging to current student
            // --------------------------------------------------------

            var checklist = await _context.StudentChecklists
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    c.UserId == userId);

            if (checklist == null)
            {
                return NotFound();
            }


            // --------------------------------------------------------
            // Validate checklist name
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError(
                    "name",
                    "Please enter a name for your checklist.");

                return View(checklist);
            }


            // --------------------------------------------------------
            // Update checklist name
            // --------------------------------------------------------

            checklist.Name = name.Trim();


            // --------------------------------------------------------
            // Selected topics
            // --------------------------------------------------------

            selectedTopics ??= new List<string>();


            // --------------------------------------------------------
            // Get all available topic definitions
            // --------------------------------------------------------

            var availableTopics = GetChecklistTopics();


            // --------------------------------------------------------
            // Create a unique key for each topic
            //
            // Category + Title is used because "Transport" exists
            // in both Before Coming and After Arriving.
            // --------------------------------------------------------

            var selectedTopicKeys = selectedTopics
                .ToHashSet();


            // --------------------------------------------------------
            // Remove topics that the student no longer wants
            //
            // IMPORTANT:
            // Existing items that remain selected are NOT recreated.
            // Therefore their IsCompleted value is preserved.
            // --------------------------------------------------------

            var itemsToRemove = checklist.Items
                .Where(item =>
                    !selectedTopicKeys.Contains(
                        $"{item.Category}|{item.Title}"))
                .ToList();

            if (itemsToRemove.Any())
            {
                _context.ChecklistItems.RemoveRange(itemsToRemove);
            }


            // --------------------------------------------------------
            // Add newly selected topics
            // --------------------------------------------------------

            var existingKeys = checklist.Items
                .Where(item => !itemsToRemove.Contains(item))
                .Select(item =>
                    $"{item.Category}|{item.Title}")
                .ToHashSet();


            var newItems = availableTopics
                .Where(topic =>
                    selectedTopicKeys.Contains(
                        $"{topic.Category}|{topic.Title}") &&
                    !existingKeys.Contains(
                        $"{topic.Category}|{topic.Title}"))
                .Select(topic => new ChecklistItem
                {
                    UserId = userId,
                    StudentChecklistId = checklist.Id,
                    Category = topic.Category,
                    Title = topic.Title,
                    Description = topic.Description,
                    IsCompleted = false
                })
                .ToList();


            if (newItems.Any())
            {
                _context.ChecklistItems.AddRange(newItems);
            }


            // --------------------------------------------------------
            // Save changes
            // --------------------------------------------------------

            await _context.SaveChangesAsync();


            // --------------------------------------------------------
            // Return to checklist
            // --------------------------------------------------------

            return RedirectToAction(
                "ViewChecklist",
                "Checklist",
                new
                {
                    area = "Student",
                    id = checklist.Id
                });
        }


        // ============================================================
        // CHECKLIST TOPIC DEFINITIONS
        // ============================================================

        private List<ChecklistTopic> GetChecklistTopics()
        {
            return new List<ChecklistTopic>
            {
                // ====================================================
                // BEFORE COMING TO NEW ZEALAND
                // ====================================================

                new ChecklistTopic
                {
                    Category = "Before Coming to New Zealand",
                    Title = "Choosing a University / Tertiary Institute",
                    Description = "Research universities and tertiary institutes and choose a suitable study option."
                },

                new ChecklistTopic
                {
                    Category = "Before Coming to New Zealand",
                    Title = "Offer Letter Process",
                    Description = "Understand the offer letter process and complete the required steps."
                },

                new ChecklistTopic
                {
                    Category = "Before Coming to New Zealand",
                    Title = "Visa Application",
                    Description = "Prepare and submit your New Zealand student visa application."
                },

                new ChecklistTopic
                {
                    Category = "Before Coming to New Zealand",
                    Title = "Medical Insurance",
                    Description = "Arrange suitable medical and travel insurance for your stay in New Zealand."
                },

                new ChecklistTopic
                {
                    Category = "Before Coming to New Zealand",
                    Title = "Preparing Documents",
                    Description = "Prepare and organise the important documents you will need before travelling."
                },

                new ChecklistTopic
                {
                    Category = "Before Coming to New Zealand",
                    Title = "Packing Guideline",
                    Description = "Prepare the items and belongings you need to bring to New Zealand."
                },

                new ChecklistTopic
                {
                    Category = "Before Coming to New Zealand",
                    Title = "Customs Regulations",
                    Description = "Learn about New Zealand customs and biosecurity requirements."
                },

                new ChecklistTopic
                {
                    Category = "Before Coming to New Zealand",
                    Title = "Budget Planning",
                    Description = "Plan your expected study, accommodation, transport and living expenses."
                },

                new ChecklistTopic
                {
                    Category = "Before Coming to New Zealand",
                    Title = "Accommodation Planning",
                    Description = "Research and arrange suitable accommodation before arriving."
                },

                new ChecklistTopic
                {
                    Category = "Before Coming to New Zealand",
                    Title = "Airport Pickup / Arrival Transport",
                    Description = "Arrange or understand how you will travel from the airport to your accommodation when you first arrive in New Zealand."
                },


                // ====================================================
                // AFTER ARRIVING IN NEW ZEALAND
                // ====================================================

                new ChecklistTopic
                {
                    Category = "After Arriving in New Zealand",
                    Title = "Banking",
                    Description = "Set up a New Zealand bank account for your everyday financial needs."
                },

                new ChecklistTopic
                {
                    Category = "After Arriving in New Zealand",
                    Title = "IRD Registration",
                    Description = "Learn about IRD registration and your tax responsibilities in New Zealand."
                },

                new ChecklistTopic
                {
                    Category = "After Arriving in New Zealand",
                    Title = "SIM Card",
                    Description = "Choose a suitable mobile provider and arrange a New Zealand SIM card."
                },

                new ChecklistTopic
                {
                    Category = "After Arriving in New Zealand",
                    Title = "Healthcare",
                    Description = "Learn how to access healthcare services in New Zealand."
                },

                new ChecklistTopic
                {
                    Category = "After Arriving in New Zealand",
                    Title = "Transport in New Zealand",
                    Description = "Learn how to use public transport and other local transport options for everyday travel in New Zealand."
                },

                new ChecklistTopic
                {
                    Category = "After Arriving in New Zealand",
                    Title = "Driver Licence",
                    Description = "Understand the requirements for driving and obtaining or converting a driver licence."
                },

                new ChecklistTopic
                {
                    Category = "After Arriving in New Zealand",
                    Title = "Employment",
                    Description = "Learn about finding work and employment requirements for international students."
                },

                new ChecklistTopic
                {
                    Category = "After Arriving in New Zealand",
                    Title = "CV Preparation",
                    Description = "Prepare a suitable New Zealand-style CV for job applications."
                },

                new ChecklistTopic
                {
                    Category = "After Arriving in New Zealand",
                    Title = "Interview Tips",
                    Description = "Learn useful interview preparation and techniques."
                },

                new ChecklistTopic
                {
                    Category = "After Arriving in New Zealand",
                    Title = "Living Expenses",
                    Description = "Understand and plan for everyday living expenses in New Zealand."
                },

                new ChecklistTopic
                {
                    Category = "After Arriving in New Zealand",
                    Title = "Community Engagement",
                    Description = "Explore university workshops, orientation events, cultural activities and support services."
                },


                // ====================================================
                // COMING WITH FAMILY
                // ====================================================

                new ChecklistTopic
                {
                    Category = "Coming With Family",
                    Title = "Partner Employment",
                    Description = "Learn about employment options and requirements for your partner."
                },

                new ChecklistTopic
                {
                    Category = "Coming With Family",
                    Title = "Childcare",
                    Description = "Research childcare options and arrangements for children."
                },

                new ChecklistTopic
                {
                    Category = "Coming With Family",
                    Title = "School Enrolment",
                    Description = "Learn about school options and enrolment requirements for children."
                },

                new ChecklistTopic
                {
                    Category = "Coming With Family",
                    Title = "Health Services",
                    Description = "Find suitable health services for family members."
                },

                new ChecklistTopic
                {
                    Category = "Coming With Family",
                    Title = "Recreation",
                    Description = "Explore recreation, activities and community opportunities for your family."
                },

                new ChecklistTopic
                {
                    Category = "Coming With Family",
                    Title = "Legal Procedure",
                    Description = "Learn about relevant legal procedures, including accommodation sharing and visa conditions."
                }
            };
        }


        // ============================================================
        // HELPER CLASS
        // ============================================================

        private class ChecklistTopic
        {
            public string Category { get; set; } = string.Empty;

            public string Title { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;
        }

        }
    }

