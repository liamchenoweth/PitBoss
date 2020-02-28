using System;
using System.Net;
using System.Reflection;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using PitBoss;
using PitBoss.Utils;
using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;

namespace BossContainer
{
    public class Boss
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            Assembly _callingAssembly = Assembly.GetExecutingAssembly();
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
                    
                    var mvcOptions = services.AddControllers();
                    mvcOptions.PartManager.ApplicationParts.Clear();
                    mvcOptions.AddApplicationPart(_callingAssembly);
                    mvcOptions.AddJsonOptions(opts => opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
                    services.AddSingleton<IContainerManager, DefaultContainerManager>();
                    services.AddSingleton<IContainerBalancer, DefaultContainerBalancer>();
                })
                .UseStartup<BossWebServerConfiguration>();

            return host;
        }
    }
}
