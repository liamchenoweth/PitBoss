using System;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Text.Json;

namespace PitBoss
{
    public class BossWebServer {

        public static IWebHostBuilder CreateHostBuilder(string[] args){

        var config = new ConfigurationBuilder()
            .AddJsonFile("configuration/defaultConfiguration.json", false, true)
            // Allow provided configuration from user
            // TODO: inform this location some other way (maybe env var?)
            .AddJsonFile("configuration/configuration.json", true, true)
            .Build();

        return new WebHostBuilder()
            .UseConfiguration(config)
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
            .UseStartup<BossWebServerConfiguration>();
        }
    }
}