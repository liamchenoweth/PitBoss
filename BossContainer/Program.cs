using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using PitBoss.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace PitBoss
{
    public class Boss
    {
        public async static Task Main(string[] args)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            var configPath = Environment.GetEnvironmentVariable("PITBOSS_CONFIGURATION");
            var config = new ConfigurationBuilder()
                .AddJsonFile(Path.IsPathRooted(configPath) ? configPath : $"{FileUtils.GetBasePath()}/{configPath}", false, true)
                .Build();
            var boss = new BossContainer(config);
            await boss.StartServiceServer(config.GetValue<int>("OperationGroupContainer:Port"), source.Token);
        }
    }

    public class BossContainer
    {

        private string _currentPipelineHash;
        private IConfiguration _configuration;
        private string PipelineLocation { 
            get {
                var pipelineLocation = _configuration["Boss:Pipelines:Location"];
                if(!Path.IsPathRooted(pipelineLocation))
                {
                    pipelineLocation = $"{FileUtils.GetBasePath()}/{pipelineLocation}";
                }
                return pipelineLocation;
            }
        }

        private string ScriptsLocation { 
            get {
                var scriptsLocation = _configuration["Boss:Scripts:Location"];
                if(!Path.IsPathRooted(scriptsLocation))
                {
                    scriptsLocation = $"{FileUtils.GetBasePath()}/{scriptsLocation}";
                }
                return scriptsLocation;
            }
        }

        private List<string> AdditionalLocation { 
            get {
                var additionalLocation = new List<string>();
                _configuration.Bind("Boss:Scripts:AdditionalLocations", additionalLocation);
                if(!additionalLocation.Any()) return new List<string>();
                return additionalLocation.Select(x => Path.IsPathRooted(x) ? x : $"{FileUtils.GetBasePath()}/{x}").ToList();
            }
        }

        private List<string> AllScriptLocations {
            get {
                var pipelineLocation = PipelineLocation;
                var scripts = ScriptsLocation;
                var additional = AdditionalLocation;
                var ret = new List<string>() { pipelineLocation, scripts };
                ret.AddRange(additional);
                return ret;
            }
        }

        public BossContainer(IConfiguration config)
        {
            _configuration = config;
        }

        private async Task<bool> CheckPipelinesUpdated(List<string> directories) {
            var hashs = await Task.WhenAll(directories.Select(async x => $"{x}:{await FileUtils.GetDirectoryHash(x)}"));
            var newHash = FileUtils.Sha256Hash(string.Join(',',hashs));
            var change = newHash != _currentPipelineHash;
            _currentPipelineHash = newHash;
            return change;
        }

        public async Task StartServiceServer(int port, CancellationToken token)
        {
            BossWebServer server = new BossWebServer();
            var host = server.StartWebHost("0.0.0.0", port);
            var logger = host.Services.GetService<ILogger<Boss>>();
            logger.LogInformation($"Starting new Container Service server on url: http://localhost:{port}");
            // Compile pipelines
            var pipelineLocation = host.Services.GetService<IConfiguration>()["Boss:Pipelines:Location"];
            if(!Path.IsPathRooted(pipelineLocation))
            {
                pipelineLocation = $"{FileUtils.GetBasePath()}/{pipelineLocation}";
            }
            var hostTask = host.WaitForShutdownAsync();
            while(!hostTask.IsCompleted)
            {
                if(await CheckPipelinesUpdated(AllScriptLocations))
                {
                    await host.Services.GetService<IPipelineManager>().CompilePipelinesAsync(pipelineLocation);
                }
                await Task.Delay(5000);
            }
            //await host.WaitForShutdownAsync();
        }

        #if LOCAL_DEV
        public async Task StartServiceServer(int port, IDistributedService memoryService, CancellationToken token)
        {
            BossWebServer server = new BossWebServer();
            server.ConfigureDistributedService(memoryService);
            var host = server.StartWebHost("0.0.0.0", port);
            var logger = host.Services.GetService<ILogger<Boss>>();
            logger.LogInformation($"Starting new Container Service server on url: http://localhost:{port}");
            // Compile pipelines
            var pipelineLocation = host.Services.GetService<IConfiguration>()["Boss:Pipelines:Location"];
            if(!Path.IsPathRooted(pipelineLocation))
            {
                pipelineLocation = $"{FileUtils.GetBasePath()}/{pipelineLocation}";
            }
            //await host.Services.GetService<IPipelineManager>().CompilePipelinesAsync(pipelineLocation);
            var hostTask = host.WaitForShutdownAsync();
            while(!hostTask.IsCompleted)
            {
                if(await CheckPipelinesUpdated(AllScriptLocations))
                {
                    await host.Services.GetService<IPipelineManager>().CompilePipelinesAsync(pipelineLocation);
                }
                await Task.Delay(5000);
            }
        }
        #endif
    }
}
