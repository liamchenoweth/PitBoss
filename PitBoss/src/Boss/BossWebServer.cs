using System;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Serilog;
using PitBoss.Utils;

namespace PitBoss
{
    public class BossWebServer {

        private Action<IServiceCollection> _containerManager;

        public BossWebServer()
        {
            StartUp();
        }

        public IWebHost StartWebHost(string[] args)
        {
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
                    var urls = new List<string>();
                    var kestrelSettings = config.GetSection("Kestrel");
                    kestrelSettings.Bind("Urls", urls);
                    foreach(var url in urls){
                        Uri uri = new Uri(url);
                        if(uri.Host == "localhost"){
                            options.ListenLocalhost(uri.Port);
                        }else {
                            options.Listen(IPAddress.Parse(uri.Host), uri.Port);
                        }
                    }
                })
                .ConfigureServices(services => {
                    if(_containerManager != null)
                    {
                        _containerManager(services);
                    }
                })
                .UseStartup<BossWebServerConfiguration>()
                .Build();

            host.Start();
            return host;
        }

        public BossWebServer UseContainerManager<T>() where T : class, IContainerManager
        {
            _containerManager = (services) => {
                services.UseContainerManager<T>();
            };
            return this;
        }

        public BossWebServer UseContainerBalancer<T>() where T : class, IContainerBalancer
        {
            _containerManager = (services) => {
                services.UseContainerBalancer<T>();
            };
            return this;
        }

        private void StartUp() 
        {
            
        }
    }
}