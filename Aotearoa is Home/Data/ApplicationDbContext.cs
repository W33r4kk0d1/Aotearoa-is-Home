using Aotearoa_is_Home.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Data
{
    public class ApplicationDbContext 
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Language> Languages { get; set; }

        public DbSet<StudentProfile> StudentProfiles { get; set; }

        public DbSet<FamilyProfile> FamilyProfiles { get; set; }

        public DbSet<AdminProfile> AdminProfiles { get; set; }

        public DbSet<EventProviderProfile> EventProviderProfiles { get; set; }

        public DbSet<SettlementInformation> SettlementInformation { get; set; }

        public DbSet<SettlementPage> SettlementPages { get; set; }

        public DbSet<ContentBlock> ContentBlocks { get; set; }

        public DbSet<Aotearoa_is_Home.Models.ServiceProvider> ServiceProviders { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<ChecklistTask> ChecklistTasks { get; set; }

        // Stores public holidays and important calendar reminders.
        public DbSet<CalendarInformation> CalendarInformations { get; set; }

        public DbSet<EventResponse> EventResponses { get; set; }

        public DbSet<EventView> EventViews { get; set; }

        public DbSet<PendingStudentRegistration> PendingStudentRegistrations { get; set; }
        public DbSet<ChecklistItem> ChecklistItems { get; set; }
        public DbSet<StudentChecklist> StudentChecklists { get; set; }
        public DbSet<EventFavourite> EventFavourites { get; set; }

        public DbSet<StudentFamilyMember> StudentFamilyMembers { get; set; }

        protected override void OnModelCreating(
            ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<StudentProfile>()
                .HasOne(s => s.User)
                .WithOne()
                .HasForeignKey<StudentProfile>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FamilyProfile>()
                .HasOne(f => f.User)
                .WithOne()
                .HasForeignKey<FamilyProfile>(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AdminProfile>()
                .HasOne(a => a.User)
                .WithOne()
                .HasForeignKey<AdminProfile>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EventProviderProfile>()
                .HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<EventProviderProfile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // EVENT RESPONSE
            builder.Entity<EventResponse>()
                .HasOne(r => r.Event)
                .WithMany()
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<EventResponse>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<EventResponse>()
                .HasIndex(r => new
                {
                    r.EventId,
                    r.UserId
                })
                .IsUnique();


            // EVENT VIEW
            builder.Entity<EventView>()
                .HasOne(v => v.Event)
                .WithMany()
                .HasForeignKey(v => v.EventId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<EventView>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EventFavourite>()
            .HasOne(f => f.Event)
            .WithMany()
            .HasForeignKey(f => f.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<EventFavourite>()
            .HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EventFavourite>()
            .HasIndex(f => new { f.EventId, f.UserId })
            .IsUnique();
        
        builder.Entity<StudentFamilyMember>()
            .HasOne(f => f.StudentUser)
            .WithMany()
            .HasForeignKey(f => f.StudentUserId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.Entity<StudentFamilyMember>()
            .HasOne(f => f.RegisteredUser)
            .WithMany()
            .HasForeignKey(f => f.RegisteredUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentFamilyMember>()
            .HasIndex(f => f.Email)
            .IsUnique();

        builder.Entity<StudentFamilyMember>()
            .HasIndex(f => f.RegisteredUserId)
            .IsUnique()
            .HasFilter("[RegisteredUserId] IS NOT NULL");
        
        builder.Entity<StudentChecklist>()
            .HasMany(c => c.Items)
            .WithOne(i => i.StudentChecklist)
            .HasForeignKey(i => i.StudentChecklistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<StudentChecklist>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    public static async Task SeedSettlementData(ApplicationDbContext context)
    {
        // Check whether Healthcare already exists.
        var healthcarePage = await context.SettlementPages
            .Include(p => p.ContentBlocks)
            .FirstOrDefaultAsync(p => p.CategoryName == "Healthcare");

        // Do not create duplicates if the page already exists.
        if (healthcarePage != null)
        {
            return;
        }

        healthcarePage = new SettlementPage
        {
            CategoryName = "Healthcare",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.SettlementPages.Add(healthcarePage);

        await context.SaveChangesAsync();

        var blocks = new List<ContentBlock>
        {
            // 1. Understanding NZ Healthcare

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "heading",
                Content = "Understanding NZ Healthcare",
                Details = "",
                DisplayOrder = 1
            },

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "subheading",
                Content = "How the New Zealand Healthcare System Works",
                Details = "",
                DisplayOrder = 2
            },

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "paragraph",
                Content = "New Zealand has a combination of publicly funded and privately provided healthcare services. The level of government funding and eligibility depends on a person's citizenship, residency or visa status. International students should check whether they are eligible for publicly funded healthcare and understand what costs they may need to pay themselves.",
                Details = "",
                DisplayOrder = 3
            },

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "link",
                Content = "Immigration New Zealand || https://www.immigration.govt.nz/live/setting-up-your-life-in-new-zealand/getting-health-care-and-finding-a-doctor/",
                Details = "",
                DisplayOrder = 4
            },

            // -------------------------------------------------
            // 2. Public Healthcare
            // -------------------------------------------------

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "heading",
                Content = "Public Healthcare",
                Details = "",
                DisplayOrder = 5
            },

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "paragraph",
                Content = "Public healthcare services are funded or subsidised by the New Zealand Government for people who meet eligibility requirements. People who are not eligible can still use public healthcare services but will generally need to pay for the services they receive.",
                Details = "",
                DisplayOrder = 6
            },

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "link",
                Content = "Immigration New Zealand",
                Details = "https://www.immigration.govt.nz/live/setting-up-your-life-in-new-zealand/getting-health-care-and-finding-a-doctor/who-can-get-public-health-care/",
                DisplayOrder = 7
            },

            // -------------------------------------------------
            // 3. Private Healthcare
            // -------------------------------------------------

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "heading",
                Content = "Private Healthcare",
                Details = "",
                DisplayOrder = 8
            },

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "paragraph",
                Content = "Private healthcare allows people to pay directly for healthcare services or use private health insurance. Private services may provide more choice over healthcare providers and appointment times, but costs depend on the service and insurance policy.",
                Details = "",
                DisplayOrder = 9
            },

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "link",
                Content = "Immigration New Zealand",
                Details = "https://www.immigration.govt.nz/live/setting-up-your-life-in-new-zealand/getting-health-care-and-finding-a-doctor/",
                DisplayOrder = 10
            },

            // -------------------------------------------------
            // 4. Primary Healthcare
            // -------------------------------------------------

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "heading",
                Content = "Primary Healthcare",
                Details = "",
                DisplayOrder = 11
            },

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "paragraph",
                Content = "Primary healthcare is usually the first point of contact when you have a health problem. It includes services such as GPs, nurses, pharmacies and other community healthcare providers. If you are enrolled with a GP, they can provide ongoing healthcare and refer you to other healthcare professionals when necessary.",
                Details = "",
                DisplayOrder = 12
            },

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "link",
                Content = "Health New Zealand – Te Whatu Ora",
                Details = "https://www.healthnz.govt.nz/",
                DisplayOrder = 13
            },

            // -------------------------------------------------
            // 5. Secondary Healthcare
            // -------------------------------------------------

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "heading",
                Content = "Secondary Healthcare",
                Details = "",
                DisplayOrder = 14
            },

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "paragraph",
                Content = "Secondary healthcare generally refers to specialist and hospital services. A GP or another healthcare professional may refer you to a specialist or hospital when more specialised assessment or treatment is required.",
                Details = "",
                DisplayOrder = 15
            },

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "link",
                Content = "Health New Zealand – Hospitals and services",
                Details = "https://www.healthnz.govt.nz/hospitals-services",
                DisplayOrder = 16
            },

            // -------------------------------------------------
            // 6. Emergency Healthcare
            // -------------------------------------------------

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "heading",
                Content = "Emergency Healthcare",
                Details = "",
                DisplayOrder = 17
            },

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "paragraph",
                Content = "Emergency healthcare is for serious or life-threatening illness and injuries. In a life-threatening emergency, call 111 and ask for an ambulance. Emergency Departments are available for urgent medical care.",
                Details = "",
                DisplayOrder = 18
            },

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "link",
                Content = "Emergency medical help – Health New Zealand",
                Details = "https://www.healthnz.govt.nz/health-topics/tests-and-treatments/emergencies-and-first-aid/emergency-medical-help",
                DisplayOrder = 19
            },

            // -------------------------------------------------
            // 7. Understanding Healthcare Costs
            // -------------------------------------------------

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "heading",
                Content = "Understanding Healthcare Costs",
                Details = "",
                DisplayOrder = 20
            },

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "paragraph",
                Content = "Healthcare costs depend on your eligibility, the type of service, the healthcare provider and whether you have insurance. International students should not assume that healthcare will be free and should check their eligibility and insurance policy before receiving non-emergency treatment.",
                Details = "",
                DisplayOrder = 21
            },

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "link",
                Content = "Healthcare costs and eligibility – Immigration New Zealand",
                Details = "https://www.immigration.govt.nz/live/setting-up-your-life-in-new-zealand/getting-health-care-and-finding-a-doctor/",
                DisplayOrder = 22
            },

            // -------------------------------------------------
            // 8. Healthcare for International Students
            // -------------------------------------------------

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "heading",
                Content = "Healthcare for International Students",
                Details = "",
                DisplayOrder = 23
            },

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "paragraph",
                Content = "Many international students are required to have medical and travel insurance as a condition of their student visa. Fee-paying student visa holders must meet the applicable insurance requirements for their visa and education provider.",
                Details = "",
                DisplayOrder = 24
            },

            new ContentBlock
            {
                SettlementPageId = healthcarePage.Id,
                Type = "link",
                Content = "Fee Paying Student Visa – Immigration New Zealand",
                Details = "https://www.immigration.govt.nz/visas/fee-paying-student-visa/",
                DisplayOrder = 25
            }
        };

        context.ContentBlocks.AddRange(blocks);

        await context.SaveChangesAsync();
    }
    }
}