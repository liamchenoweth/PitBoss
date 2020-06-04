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
    public class SqliteContext : BossContext
    {

        public SqliteContext(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite(_configuration["Boss:Database:SQLite"]);
        }
    }

    public class SqliteContextFactory : IDesignTimeDbContextFactory<SqliteContext>
    {
        public SqliteContext CreateDbContext(string[] args)
        {
            var configPath = Environment.GetEnvironmentVariable("PITBOSS_CONFIGURATION");
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.IsPathRooted(configPath) ? configPath : $"{FileUtils.GetBasePath()}/{configPath}", true, true)
                .Build();
            return new SqliteContext(configuration);
        }
    }
}