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

        // Stores public holidays and important calendar reminders.
        public DbSet<CalendarInformation> CalendarInformations { get; set; }

        public DbSet<EventResponse> EventResponses { get; set; }

        public DbSet<EventView> EventViews { get; set; }

        public DbSet<PendingStudentRegistration> PendingStudentRegistrations { get; set; }
        public DbSet<ChecklistItem> ChecklistItems { get; set; }

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
        }
    }
}