using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PitBoss.Utils;

namespace PitBoss
{
    public class MSSQLContext : BossContext
    {

        public MSSQLContext(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var stringBuilder = new SqlConnectionStringBuilder();
            stringBuilder["Server"] = $"{_configuration["Boss:Database:MSSQL:Server"]},{_configuration["Boss:Database:MSSQL:Port"]}";
            stringBuilder["Database"] = _configuration["Boss:Database:MSSQL:Database"];
            stringBuilder["User Id"] = _configuration["Boss:Database:MSSQL:Username"];
            stringBuilder["Password"] = _configuration["Boss:Database:MSSQL:Password"];
            options.UseSqlServer(stringBuilder.ToString());
        }
    }

    public class MSSQLContextFactory : IDesignTimeDbContextFactory<MSSQLContext>
    {
        public MSSQLContext CreateDbContext(string[] args)
        {
            var configPath = Environment.GetEnvironmentVariable("PITBOSS_CONFIGURATION");
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.IsPathRooted(configPath) ? configPath : $"{FileUtils.GetBasePath()}/{configPath}", false, true)
                .Build();
            return new MSSQLContext(configuration);
        }
    }
}