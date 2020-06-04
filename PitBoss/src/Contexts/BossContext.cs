using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PitBoss.Utils;

namespace PitBoss
{
    public abstract class BossContext : DbContext
    {
        protected IConfiguration _configuration;
        public DbSet<PipelineRequest> PipelineRequests { get; set; }
        public DbSet<OperationRequest> OperationRequests { get; set; }
        public DbSet<OperationResponse> OperationResponses { get; set; }
        public DbSet<DistributedOperationRequest> DistributedOperationRequests { get; set; }
        public DbSet<DistributedRequestSeed> DistributedRequestSeeds { get; set; }

        public BossContext(IConfiguration configuration) : base()
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            switch(_configuration["Boss:Database:UseDatabase"])
            {
                case "Postgres":
                    var stringBuilder = new NpgsqlConnectionStringBuilder();
                    stringBuilder.Host = _configuration["Boss:Database:Postgres:Host"];
                    stringBuilder.Port = _configuration.GetValue<int>("Boss:Database:Postgres:Port");
                    stringBuilder.Database = _configuration["Boss:Database:Postgres:Database"];
                    stringBuilder.Username = _configuration["Boss:Database:Postgres:Username"];
                    stringBuilder.Password = _configuration["Boss:Database:Postgres:Password"];
                    options.UseNpgsql(stringBuilder.ToString());
                    break;
                default:
                    options.UseSqlite(_configuration["Boss:Database:SQLite"]);
                    break;
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
}