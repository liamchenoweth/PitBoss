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
    public class PostgresContext : BossContext
    {

        public PostgresContext(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var stringBuilder = new NpgsqlConnectionStringBuilder();
            stringBuilder.Host = _configuration["Boss:Database:Postgres:Host"];
            stringBuilder.Port = _configuration.GetValue<int>("Boss:Database:Postgres:Port");
            stringBuilder.Database = _configuration["Boss:Database:Postgres:Database"];
            stringBuilder.Username = _configuration["Boss:Database:Postgres:Username"];
            stringBuilder.Password = _configuration["Boss:Database:Postgres:Password"];
            options.UseNpgsql(stringBuilder.ToString());
        }
    }

    public class PostgresContextFactory : IDesignTimeDbContextFactory<PostgresContext>
    {
        public PostgresContext CreateDbContext(string[] args)
        {
            var configPath = Environment.GetEnvironmentVariable("PITBOSS_CONFIGURATION");
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.IsPathRooted(configPath) ? configPath : $"{FileUtils.GetBasePath()}/{configPath}", false, true)
                .Build();
            return new PostgresContext(configuration);
        }
    }
}