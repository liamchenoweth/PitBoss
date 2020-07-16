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
    // This is used for unit tests that require ef context
    public class InMemoryContext : BossContext
    {
        private string name;

        public InMemoryContext(string dbName) : base(null)
        {
            name = dbName;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseInMemoryDatabase(databaseName: name);
        }
    }

    public class InMemoryContextFactory : IBossContextFactory
    {
        string name;
        public InMemoryContextFactory(string dbName)
        {
            name = dbName;
        }

        public BossContext GetContext()
        {
            return new InMemoryContext(name);
        }
    }
}