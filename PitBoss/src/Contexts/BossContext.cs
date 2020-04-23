using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace PitBoss
{
    public class BossContext : DbContext
    {
        private string _connectionString;
        private IConfiguration _configuration;
        public DbSet<PipelineRequest> PipelineRequests { get; set; }
        public DbSet<OperationRequest> OperationRequests { get; set; }
        public DbSet<OperationResponse> OperationResponses { get; set; }
        public DbSet<DistributedOperationRequest> DistributedOperationRequests { get; set; }
        public DbSet<DistributedRequestSeed> DistributedRequestSeeds { get; set; }

        public BossContext(IConfiguration configuration) : base()
        {
            _configuration = configuration;
        }

        public BossContext(string connectionString) : base()
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if(!string.IsNullOrEmpty(_connectionString))
            {
                options.UseSqlite(_connectionString);
            }
            else
            {
                options.UseSqlite(_configuration["Boss:Database:ConnectionString"]);
            }
        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && (
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = entityEntry.Entity as BaseEntity;
                if(entity == null) continue;
                entity.Updated = DateTime.Now;

                if (entityEntry.State == EntityState.Added)
                {
                    entity.Created = DateTime.Now;
                }
            }

            return base.SaveChanges();
        }
    }

    public class BossContextFactory : IDesignTimeDbContextFactory<BossContext>
    {
        public BossContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("configuration/defaultConfiguration.json", false, true).Build();
            if(configuration.GetValue<bool>("Library"))
            {
                return new BossContext("library.db");
            }
            return new BossContext(configuration);
        }
    }
}