using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            services.AddSingleton<IPipelineManager, DefaultPipelineManager>();
            services.AddSingleton<IDistributedService, MemoryDistributedService>();
            services.AddSingleton(providers);
            services.AddSingleton<ILoggerFactory>(sc => {
                var providerCollection = sc.GetService<LoggerProviderCollection>();
                var factory = new SerilogLoggerFactory(null, true, providerCollection);

                foreach (var provider in sc.GetServices<ILoggerProvider>())
                    factory.AddProvider(provider);

                return factory;
            });

            services.AddTransient<IPipelineRequestManager, DefaultPipelineRequestManager>();
            services.AddTransient<IOperationRequestManager, DefaultOperationRequestManager>();

            services.AddRouting();
            services.AddHttpClient();
            services.AddDbContext<BossContext>();
            services.AddLogging(l => l.AddConsole());
            // Hosted Services
            services.AddHostedService<OperationGroupService>();
            services.AddHostedService<ContainerService>();
            services.AddControllers().AddNewtonsoftJson(options => {
              options.SerializerSettings.Converters.Add(new StringEnumConverter());  
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}