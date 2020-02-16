using System;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text.Json;
using Serilog;
using PitBoss.Utils;

namespace PitBoss
{
    public class OperationWebServer {

        private Assembly _callingAssembly;

        public OperationWebServer()
        {
            StartUp();
        }

        public IWebHost StartWebHost(int port)
        {
            _callingAssembly = Assembly.GetCallingAssembly();
            var config = new ConfigurationBuilder()
                .AddJsonFile("configuration/defaultConfiguration.json", false, true)
                // Allow provided configuration from user
                // TODO: inform this location some other way (maybe env var?)
                .AddJsonFile("configuration/configuration.json", true, true)
                .Build();

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseSerilog(LoggingUtils.ConfigureSerilog())
                .UseKestrel(options => {
                    options.Listen(new IPAddress(new byte[] {127,0,0,1}), port);
                })
                .ConfigureServices(services => {
                    var mvcOptions = services.AddControllers();
                    mvcOptions.PartManager.ApplicationParts.Clear();
                    mvcOptions.AddApplicationPart(_callingAssembly);
                })
                .UseStartup<OperationWebServerConfiguration>()
                .Build();

            host.Start();
            return host;
        }

        private void StartUp() 
        {
            
        }
    }
}