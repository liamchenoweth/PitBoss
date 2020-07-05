using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PitBoss.Utils;

namespace PitBoss
{
    public class PipelineToStepMapper
    {
        public string Version {get;set;}
        public PipelineModel Pipeline { get; set; }
        public string StepHash {get;set;}
        public PipelineStepModel Step {get; set;}
        public int Order { get; set; }
    }

    public abstract class BossContext : DbContext
    {
        protected IConfiguration _configuration;
        public DbSet<PipelineRequest> PipelineRequests { get; set; }
        public DbSet<OperationRequest> OperationRequests { get; set; }
        public DbSet<OperationResponse> OperationResponses { get; set; }
        public DbSet<DistributedOperationRequest> DistributedOperationRequests { get; set; }
        public DbSet<DistributedRequestSeed> DistributedRequestSeeds { get; set; }
        public DbSet<PipelineStepModel> PipelineSteps { get; set; }
        public DbSet<PipelineModel> Pipelines { get; set; }
        public DbSet<PipelineToStepMapper> PipelineStepMap {get; set;}

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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // var stringSaver = new ValueConverter<List<string>, List<StringWrapper>>(
            //     x => x.Select(y => new StringWrapper(y)).ToList(),
            //     x => x.Select(y => y.Value).ToList());
            // builder.Entity<PipelineStep>().Property(e => e.NextSteps).HasConversion(stringSaver);
            var stringSaver = new ValueConverter<List<string>, string>(
                x => string.Join("<sep>", x),
                x => x.Split("<sep>", StringSplitOptions.RemoveEmptyEntries).ToList());
            builder.Entity<PipelineStepModel>().Property(e => e.NextSteps).HasConversion(stringSaver);
            builder.Entity<PipelineToStepMapper>()
                .HasOne(x => x.Pipeline)
                .WithMany(x => x.Steps)
                .HasForeignKey(x => x.Version);
            builder.Entity<PipelineToStepMapper>()
                .HasOne(x => x.Step)
                .WithMany(x => x.Pipelines)
                .HasForeignKey(x => x.StepHash);
            builder.Entity<PipelineToStepMapper>().HasKey(x => new {x.StepHash, x.Version});
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

    public interface IBossContextFactory
    {
        BossContext GetContext();
    }

    public class BossContextFactory<TContext> : IBossContextFactory where TContext : BossContext
    {
        private IConfiguration _configuration;
        public BossContextFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public BossContext GetContext()
        {
            return (BossContext) Activator.CreateInstance(typeof(TContext), new object[]{_configuration});
        }
    }
}