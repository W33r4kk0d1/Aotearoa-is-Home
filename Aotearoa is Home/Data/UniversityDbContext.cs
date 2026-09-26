using Aotearoa_is_Home.Models;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Data
{
    public class UniversityDbContext : DbContext
    {
        public UniversityDbContext( DbContextOptions<UniversityDbContext> options)
            : base(options)
        {
        }

        public DbSet<UniversityStudent> UniversityStudents { get; set; }

        public DbSet<Employee> Employees { get; set; }
        
        public DbSet<Organization> Organizations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Organization)
                .WithMany(o => o.Employees)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}