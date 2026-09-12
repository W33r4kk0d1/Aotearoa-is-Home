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
        }
    }
}