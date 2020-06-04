using System;
using System.IO;
using System.Net;
using System.Reflection;
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

        private Assembly _callingAssembly;
        private Action<IServiceCollection> _containerManager;
        private Action<IServiceCollection> _containerBalancer;
        private Action<IServiceCollection> _configureServices;
        private Action<IServiceCollection> _configureDistributedService;
        public string Host {get; private set;}
        public int Port {get; private set;}

        public IWebHost StartWebHost(string host, int port)
        {
            _callingAssembly = Assembly.GetCallingAssembly();
            var configPath = Environment.GetEnvironmentVariable("PITBOSS_CONFIGURATION");
            var config = new ConfigurationBuilder()
                .AddJsonFile(Path.IsPathRooted(configPath) ? configPath : $"{FileUtils.GetBasePath()}/{configPath}", false, true)
                .Build();

            Host = host;
            Port = port;

            var webHost = new WebHostBuilder()
                .UseConfiguration(config)
                .UseSerilog(LoggingUtils.ConfigureSerilog())
                .UseKestrel(options => {
                    options.Listen(IPAddress.Parse(Host), Port);
                })
                .ConfigureServices(services => {
                    
                    var mvcOptions = services.AddControllers();
                    mvcOptions.PartManager.ApplicationParts.Clear();
                    mvcOptions.AddApplicationPart(_callingAssembly);

                    if(_containerManager != null) _containerManager(services);
                    if(_containerBalancer != null) _containerBalancer(services);
                    if(_configureServices != null) _configureServices(services);
                    if(_configureDistributedService != null) _configureDistributedService(services);
                })
                .UseStartup<BossWebServerConfiguration>()
                .Build();

            webHost.Start();
            return webHost;
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
            _containerBalancer = (services) => {
                services.UseContainerBalancer<T>();
            };
            return this;
        }

        public BossWebServer ConfigureDistributedService<T>(T obj = null) where T : class, IDistributedService
        {
            if(obj != null) _configureDistributedService = (services) => services.AddSingleton<IDistributedService>(obj);
            else _configureDistributedService = (services) => services.AddSingleton<IDistributedService, T>();
            return this;
        }

        public BossWebServer ConfigureServices(Action<IServiceCollection> action) { _configureServices = action; return this;}
    }
}