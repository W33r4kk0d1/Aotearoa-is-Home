using Aotearoa_is_Home.Models;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SettlementInformation> SettlementInformation { get; set; }
        public DbSet<SettlementPage> SettlementPages { get; set; }
        public DbSet<ContentBlock> ContentBlocks { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Seed the Category Pages (Fulfills FR-IM-14 to FR-IM-23)
            modelBuilder.Entity<SettlementPage>().HasData(
                new SettlementPage { Id = 1, CategoryName = "Accommodation" },
                new SettlementPage { Id = 2, CategoryName = "Education" },
                new SettlementPage { Id = 3, CategoryName = "Healthcare" },
                new SettlementPage { Id = 4, CategoryName = "Language Assistance" },
                new SettlementPage { Id = 5, CategoryName = "Employment" },
                new SettlementPage { Id = 6, CategoryName = "Banking & Finance" },
                new SettlementPage { Id = 7, CategoryName = "Transport" },
                new SettlementPage { Id = 8, CategoryName = "Community Support" },
                new SettlementPage { Id = 9, CategoryName = "Childcare & Family" },
                new SettlementPage { Id = 10, CategoryName = "Emergency Support" }
            );

            // 2. Seed Content Blocks for the pages using your PDF text lists
            modelBuilder.Entity<ContentBlock>().HasData(
                // Accommodation Blocks (Page 1)
                new ContentBlock { 
                    Id = 1, 
                    SettlementPageId = 1, 
                    Type = "Heading", 
                    Content = "Finding Accommodation in New Zealand", 
                    DisplayOrder = 1 },
                new ContentBlock { 
                    Id = 2, 
                    SettlementPageId = 1, 
                    Type = "Paragraph", 
                    Content = "Options include Homestays, Student accommodation, Youth hostels, Shared apartments, and Rental houses.", 
                    DisplayOrder = 2 },
                new ContentBlock { 
                    Id = 3, 
                    SettlementPageId = 1, 
                    Type = "Heading", 
                    Content = "Renting Guidelines", 
                    DisplayOrder = 3 },
                new ContentBlock { 
                    Id = 4, 
                    SettlementPageId = 1, 
                    Type = "Paragraph", 
                    Content = "Ensure you understand your tenancy agreement, bond payments, and tenant rights.", 
                    DisplayOrder = 4 },

                // Education Blocks (Page 2)
                new ContentBlock { 
                    Id = 5, 
                    SettlementPageId = 2, 
                    Type = "Heading", 
                    Content = "Tertiary Information & Support", 
                    DisplayOrder = 1 },
                new ContentBlock { 
                    Id = 6, 
                    SettlementPageId = 2, 
                    Type = "Paragraph", 
                    Content = "Utilize campus international student support, academic learning hubs, library networks, and career advisory services.", 
                    DisplayOrder = 2 },

                // Healthcare Blocks (Page 3)
                new ContentBlock { 
                    Id = 7, 
                    SettlementPageId = 3, 
                    Type = "Heading", 
                    Content = "Medical Centres & Insurance", 
                    DisplayOrder = 1 },
                new ContentBlock { 
                    Id = 8, 
                    SettlementPageId = 3, 
                    Type = "Paragraph", 
                    Content = "Register with a local General Practitioner (GP). International students must maintain current medical insurance coverage.", 
                    DisplayOrder = 2 },

                // Employment Blocks (Page 5)
                new ContentBlock { 
                    Id = 9, 
                    SettlementPageId = 5, 
                    Type = "Heading", 
                    Content = "NZ Workplace Rights",
                    DisplayOrder = 1 },
                new ContentBlock { 
                    Id = 10, 
                    SettlementPageId = 5, 
                    Type = "Paragraph", 
                    Content = "All workers are entitled to minimum wage, scheduled breaks, sick leave, and protection from workplace harassment.", 
                    DisplayOrder = 2 }
            ); {
                    base.OnModelCreating(modelBuilder);

                    modelBuilder.Entity<SettlementInformation>().HasData(
                        new SettlementInformation { 
                            Id = 1, 
                            Topic = "Accommodation", 
                            Title = "Finding Accommodation", 
                            Description = "Information regarding homestays, student accommodation, flatting, and tenant rights." },
                        new SettlementInformation { 
                            Id = 2, 
                            Topic = "Education", 
                            Title = "Educational & Tertiary Information", 
                            Description = "Details about international student support, academic integrity, and university resources." },
                        new SettlementInformation { 
                            Id = 3, 
                            Topic = "Healthcare", 
                            Title = "Health Care and Wellbeing", 
                            Description = "Guidance on finding a GP, registering with a medical centre, and health insurance." },
                        new SettlementInformation { 
                            Id = 4, 
                            Topic = "Language Assistance", 
                            Title = "Language Support Resources", 
                            Description = "Everyday Kiwi words, slang, communication card prompts, and family language support." },
                        new SettlementInformation { 
                            Id = 5, 
                            Topic = "Employment", 
                            Title = "Employment Rights & Job Hunting", 
                            Description = "Information on work rights, CV/cover letter creation, interview etiquette, and workplace culture." },
                        new SettlementInformation { 
                            Id = 6, 
                            Topic = "Banking & Finance", 
                            Title = "Banking & Financial Management", 
                            Description = "Opening a bank account, everyday transaction accounts, tracking weekly budgets, and IRD tax profiles." },
                        new SettlementInformation { 
                            Id = 7, 
                            Topic = "Transport", 
                            Title = "Public Transport & Driving", 
                            Description = "Bus, train, and ferry logistics alongside NZ road rules, licensing setup, and vehicle ownership." },
                        new SettlementInformation { 
                            Id = 8, 
                            Topic = "Community Support", 
                            Title = "Social Networks & Community", 
                            Description = "Making friends, university student clubs, local community centers, and cultural activities." },
                        new SettlementInformation { 
                            Id = 9, 
                            Topic = "Childcare & Family", 
                            Title = "Childcare & School Enrolment", 
                            Description = "Daycare options, school zone applications, children's health checks, and local parenting culture." },
                        new SettlementInformation { 
                            Id = 10, 
                            Topic = "Emergency & Safety", 
                            Title = "Emergency Support & Personal Safety", 
                            Description = "How to reach services via 111, dealing with natural disasters, and household safety guidelines." },
                        new SettlementInformation { 
                            Id = 11, 
                            Topic = "Immigration & Visa", 
                            Title = "Immigration & Visa Compliance", Description = "Details regarding student visa conditions, renewal steps, and staying compliant." }
                    );
                }
        }
    }}