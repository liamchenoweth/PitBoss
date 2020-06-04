#if LOCAL_DEV
using System;
using System.Net.Http;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Newtonsoft.Json;
using PitBoss.Extensions;
using PitBoss.Utils;
using JobContainer;

namespace PitBoss {
    public class DefaultOperationContainer : IOperationContainer {

        private IHttpClientFactory _clientFactory;
        private ILogger _logger;
        private CancellationTokenSource _jobSource;
        private Task _jobTask;
        private IConfiguration _configuration;

        public DefaultOperationContainer(
            PipelineStep step, 
            IHttpClientFactory clientFactory, 
            ILogger logger, 
            IConfiguration configuration)
        {
            Operation = step.Name;
            _clientFactory = clientFactory;
            _logger = logger;
            _configuration = configuration;

            Id = Guid.NewGuid().ToString();
            Name = $"{Operation}-{Id}";
            
            var openPort = IPUtils.GetAvailablePort(1024);
            _jobSource = new CancellationTokenSource();
            _jobTask = Job.StartJobServer(openPort, _jobSource.Token);
            Url = $"http://localhost:{openPort}";
            
            var task = SendOperationAsync(step.Name);
            task.Wait();
        }

        public string Name { get; private set; }
        public string Url { get; private set; }
        public string Operation { get; private set; }
        public string Id { get; private set; }

        public async Task SendOperationAsync(string location)
        {
            var basePath = "";
            if(!Path.IsPathRooted(_configuration["Boss:Scripts:Location"])) basePath = FileUtils.GetBasePath() + "/";
            var fileName = $"{basePath}{_configuration["Boss:Scripts:Location"]}/{location}";
            var filesToSend = new List<string>();
            _configuration.Bind("Boss:Scripts:AdditionalLocations", filesToSend);
            filesToSend = filesToSend.Select(x => 
            {
                var pathRoot = "";
                if(!Path.IsPathRooted(x)) pathRoot = FileUtils.GetBasePath() + "/";
                return $"{pathRoot}{x}";
            }).SelectMany(x => Directory.GetFiles(x, "*", SearchOption.AllDirectories))
                .ToList();
            filesToSend.Add(fileName);
            var client = _clientFactory.CreateClient();
            MultipartFormDataContent content = new MultipartFormDataContent();
            foreach(var file in filesToSend)
            {
                byte[] payload = null;
                try
                {
                    payload = await File.ReadAllBytesAsync(file);
                }catch(FileNotFoundException e)
                {
                    _logger.LogCritical(e, $"Operation script not found at {file}, shutting down container");
                    _jobSource.Cancel();
                }
                content.Add(new ByteArrayContent(payload), "executionScript", file);
            }
            try
            {
                var response = await client.PostAsync($"{Url}/operation/script", content);
                response.EnsureSuccessStatusCode();
            }
            catch(Exception e)
            {
                _logger.LogCritical(e, "Was unable to send step to container, shutting down container");
                _jobSource.Cancel();
            }
        }

        public void SendOperation(string location)
        {
            SendOperationAsync(location).Wait();
        }

        public void SendRequest(OperationRequest request)
        {
            SendRequestAsync(request).Wait();
        }

        public async Task SendRequestAsync(OperationRequest request)
        {
            var client = _clientFactory.CreateClient();
            var text = JsonConvert.SerializeObject(request);
            var content = new StringContent(text, Encoding.UTF8, "application/json");
            await client.PostAsync($"{Url}/operation/request", content);
        }

        public OperationStatus Heartbeat()
        {
            var task = HeartbeatAsync();
            task.Wait();
            return task.Result;
        }

        public async Task<OperationStatus> HeartbeatAsync()
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync($"http://{Url}/heartbeat");
            return await response.Content.DeserialiseAsync<OperationStatus>();
        }

        public OperationStatus GetContainerStatus()
        {
            var task = GetContainerStatusAsync();
            task.Wait();
            return task.Result;
        }

        public async Task<OperationStatus> GetContainerStatusAsync()
        {
            var client = _clientFactory.CreateClient();
            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync($"http://{Url}/container/status");
            }catch(Exception e)
            {
                return new OperationStatus
                {
                    ContainerStatus = ContainerStatus.Error
                };
            }
            var output = await response.Content.ReadAsStringAsync();
            return await response.Content.DeserialiseAsync<OperationStatus>();
        }

        public OperationRequestStatus GetRequestStatus(OperationRequest request)
        {
            var task = GetRequestStatusAsync(request);
            task.Wait();
            return task.Result;
        }

        public async Task<OperationRequestStatus> GetRequestStatusAsync(OperationRequest request)
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync($"http://{Url}/operation/status?requestId={request.Id}");
            return await response.Content.DeserialiseAsync<OperationRequestStatus>();
        }

        public bool SendShutdown()
        {
            var task = SendShutdownAsync();
            task.Wait();
            return task.Result;
        }

        public async Task<bool> SendShutdownAsync()
        {
            var client = _clientFactory.CreateClient();
            var response = await client.PostAsync($"{Url}/shutdown", null);
            return (await response.Content.DeserialiseAsync<OperationStatus>()).ContainerStatus == ContainerStatus.ShuttingDown;
        }

        public void ForceShutdown()
        {
            ForceShutdownAsync().Wait();
        }

        public async Task ForceShutdownAsync()
        {
            _jobSource.Cancel();
        }

        public OperationDescription GetDescription()
        {
            var task = GetDescriptionAsync();
            task.Wait();
            return task.Result;
        }

        public async Task<OperationDescription> GetDescriptionAsync()
        {
            return new OperationDescription
            {
                Name = Name,
                Id = Id,
                Url = Url,
                Status = await GetContainerStatusAsync()
            };
        }
    }
}
#endif