using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Extensions.Logging;
using PitBoss.Utils;
using PitBoss.Extensions;

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
                var factory = new SerilogLoggerFactory(Log.Logger, true, providerCollection);

                foreach (var provider in sc.GetServices<ILoggerProvider>())
                    factory.AddProvider(provider);

                return factory;
            });

            ConfigureCache(services);

            services.AddSingleton<IPipelineManager, DefaultPipelineManager>();
            services.AddTransient<IDistributedRequestManager, DefaultDistributedRequestManager>();
            services.AddSingleton(providers);

            services.AddTransient<IPipelineRequestManager, DefaultPipelineRequestManager>();
            services.AddTransient<IOperationRequestManager, DefaultOperationRequestManager>();

            services.AddRouting();
            services.AddHttpClient();
            services.Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, NoiseFilter>());
            switch(_config["Boss:Database:UseDatabase"])
            {
                case "Postgres":
                    services.AddSingleton<IBossContextFactory, BossContextFactory<PostgresContext>>();
                    break;
                case "MySql":
                    services.AddSingleton<IBossContextFactory, BossContextFactory<MySqlContext>>();
                    break;
                case "MSSQL":
                    services.AddSingleton<IBossContextFactory, BossContextFactory<MSSQLContext>>();
                    break;
                case "Sqlite":
                default:
                    services.AddSingleton<IBossContextFactory, BossContextFactory<SqliteContext>>();
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