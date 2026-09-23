using Aotearoa_is_Home.Models;
using Microsoft.EntityFrameworkCore;

namespace Aotearoa_is_Home.Data
{
    public class UniversityDbContext : DbContext
    {
        public UniversityDbContext(
            DbContextOptions<UniversityDbContext> options)
            : base(options)
        {
        }

        public DbSet<UniversityStudent> UniversityStudents { get; set; }
    }
}