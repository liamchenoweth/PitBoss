using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using PitBoss.Utils;

namespace PitBoss
{
    public class OperationWebServerConfiguration {
        private IConfiguration _config;

        public OperationWebServerConfiguration(IConfiguration config) {
            _config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var providers = new LoggerProviderCollection();

            Log.Logger = LoggingUtils.ConfigureSerilog();

            services.AddRouting();
            services.AddSingleton<IOperationHealthManager, DefaultOperationHealthManager>();
            services.AddSingleton<IOperationManager, DefaultOperationManager>();
            services.AddSingleton(providers);
            services.AddSingleton<ILoggerFactory>(sc => {
                var providerCollection = sc.GetService<LoggerProviderCollection>();
                var factory = new SerilogLoggerFactory(null, true, providerCollection);

                foreach (var provider in sc.GetServices<ILoggerProvider>())
                    factory.AddProvider(provider);

                return factory;
            });
            services.AddHttpClient();
            services.AddHostedService<OperationService>();
            services.AddControllers().AddNewtonsoftJson()
            .AddJsonOptions(opts => {
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
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