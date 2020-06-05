using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Extensions.Logging;
using PitBoss.Utils;

namespace PitBoss
{
    public class BossWebServerConfiguration {
        private IConfiguration _config;

        public BossWebServerConfiguration(IConfiguration config) {
            _config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var providers = new LoggerProviderCollection();

            Log.Logger = LoggingUtils.ConfigureSerilog();
            services.AddSingleton<ILoggerFactory>(sc => {
                var providerCollection = sc.GetService<LoggerProviderCollection>();
                var factory = new SerilogLoggerFactory(null, true, providerCollection);

                foreach (var provider in sc.GetServices<ILoggerProvider>())
                    factory.AddProvider(provider);

                return factory;
            });

            ConfigureCache(services);

            services.AddSingleton<IPipelineManager, DefaultPipelineManager>();
            //services.AddSingleton<IDistributedService, MemoryDistributedService>();
            services.AddTransient<IDistributedRequestManager, DefaultDistributedRequestManager>();
            services.AddSingleton(providers);

            services.AddTransient<IPipelineRequestManager, DefaultPipelineRequestManager>();
            services.AddTransient<IOperationRequestManager, DefaultOperationRequestManager>();

            services.AddRouting();
            services.AddHttpClient();
            switch(_config["Boss:Database:UseDatabase"])
            {
                case "Postgres":
                    services.AddDbContext<BossContext, PostgresContext>(ServiceLifetime.Transient);
                    break;
                case "MySql":
                    services.AddDbContext<BossContext, MySqlContext>(ServiceLifetime.Transient);
                    break;
                case "MSSQL":
                    services.AddDbContext<BossContext, MSSQLContext>(ServiceLifetime.Transient);
                    break;
                case "Sqlite":
                default:
                    services.AddDbContext<BossContext, SqliteContext>(ServiceLifetime.Transient);
                    break;
            }
            services.AddLogging(l => l.AddConsole());
            // Hosted Services
            services.AddControllers().AddNewtonsoftJson(options => {
              options.SerializerSettings.Converters.Add(new StringEnumConverter());
              options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }

        private void ConfigureCache(IServiceCollection services)
        {
            switch(_config["Boss:Cache:UseCache"])
            {
                case "Redis":
                    services.AddSingleton<IDistributedService, RedisDistributedService>();
                    break;
                case "Memory":
                    services.AddSingleton<IDistributedService, MemoryDistributedService>();
                    break;
                case "PreCreated":
                    return;
                default:
                    services.AddSingleton<IDistributedService, MemoryDistributedService>();
                    break;
            }
        }
    }
}