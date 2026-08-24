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
    }
}