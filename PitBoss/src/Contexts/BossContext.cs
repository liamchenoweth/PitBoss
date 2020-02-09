using Microsoft.EntityFrameworkCore;

namespace PitBoss
{
    public class BossContext : DbContext
    {
        public DbSet<PipelineRequest> PipelineRequests { get; set; }
        public DbSet<OperationRequest> OperationRequests { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=database.db");
        }
    }
}