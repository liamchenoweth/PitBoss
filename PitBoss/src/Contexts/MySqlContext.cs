using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PitBoss.Utils;

namespace PitBoss
{
    public class MySqlContext : BossContext
    {

        public MySqlContext(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var stringBuilder = new MySqlConnectionStringBuilder();
            stringBuilder.Server = _configuration["Boss:Database:MySql:Server"];
            stringBuilder.Database = _configuration["Boss:Database:MySql:Database"];
            stringBuilder.UserID = _configuration["Boss:Database:MySql:Username"];
            stringBuilder.Password = _configuration["Boss:Database:MySql:Password"];
            options.UseMySQL(stringBuilder.ToString());
        }
    }

    public class MySqlContextFactory : IDesignTimeDbContextFactory<MySqlContext>
    {
        public MySqlContext CreateDbContext(string[] args)
        {
            var configPath = Environment.GetEnvironmentVariable("PITBOSS_CONFIGURATION");
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.IsPathRooted(configPath) ? configPath : $"{FileUtils.GetBasePath()}/{configPath}", false, true)
                .Build();
            return new MySqlContext(configuration);
        }
    }
}