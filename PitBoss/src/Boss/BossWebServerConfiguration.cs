using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;

namespace PitBoss
{
    public class BossWebServerConfiguration {
        private IConfiguration _config;
        private ILogger _logger;

        public BossWebServerConfiguration(IConfiguration config) {
            _config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddSingleton<IPipelineManager, DefaultPipelineManager>();
            services.AddTransient<IPipelineRequestManager, DefaultPipelineRequestManager>();
            services.AddTransient<IOperationRequestManager, DefaultOperationRequestManager>();
            // Hosted Services
            services.AddHostedService<OperationService>();
            services.AddHostedService<ContainerService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
        }
    }
}