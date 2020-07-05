using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PitBoss.Utils;
using PitBoss.Extensions;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace PitBoss
{
    public class DockerComposeOperationContainer : IOperationContainer
    {
        private IHttpClientFactory _clientFactory;
        private IConfiguration _configuration;
        private ILogger _logger;
        private DockerClient _dockerClient;
        private PipelineStep _step;

        public string Name { get; set; }
        public string Url { get; set; }
        public string Operation { get; set; }
        public string Id { get; set; }

        public DockerComposeOperationContainer(
            PipelineStep step,
            IHttpClientFactory clientFactory, 
            IConfiguration configuration, 
            ILogger logger,
            DockerClient client)
        {
            _dockerClient = client;
            _clientFactory = clientFactory;
            _configuration = configuration;
            _logger = logger;
            _step = step;

            Name = FileUtils.RandomString(8);
            Url = Name;
            Operation = step.Name;
            Id = Name;

            var createTask = _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters()
            {
                Name = Name,
                Hostname = Name,
                Image = _configuration["Operations:Image"],
                Labels = new Dictionary<string, string>()
                {
                    {"group", step.Name}
                },
                HostConfig = new HostConfig()
                {
                    NetworkMode = "pitboss"
                }
            });

            createTask.Wait();
            Id = createTask.Result.ID;

            _dockerClient.Containers.StartContainerAsync(Id, null).Wait();
            for(var x = 0; x < 4; x++)
            {
                try
                {
                    _clientFactory.CreateClient().GetAsync($"http://{Url}/container/status").Wait();
                    return;
                }
                catch(Exception)
                {
                    Task.Delay(5000).Wait();
                }
            }
            _logger.LogError("Container did not become ready forcing container shutdown");
            ForceShutdown();
            throw new Exception("Container did not become ready");
        }

        public DockerComposeOperationContainer(
            IHttpClientFactory clientFactory, 
            IConfiguration configuration, 
            ILogger logger, 
            DockerClient client)
        {
            _dockerClient = client;
            _clientFactory = clientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public void SendOperation(string location)
        {
            SendOperationAsync(location).Wait();
        }

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
                    await SendShutdownAsync();
                }
                content.Add(new ByteArrayContent(payload), "executionScript", file);
            }
            try
            {
                var response = await client.PostAsync($"http://{Url}/operation/script", content);
                response.EnsureSuccessStatusCode();
            }
            catch(Exception e)
            {
                try
                {
                    _logger.LogCritical(e, "Was unable to send step to container, shutting down container");
                    await SendShutdownAsync();
                }
                catch(Exception ex)
                {
                    _logger.LogCritical(ex, "Container not responding forcing shutdown");
                    await ForceShutdownAsync();
                }
            }
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
            await client.PostAsync($"http://{Url}/operation/request", content);
        }

        public bool SendShutdown()
        {
            var task = SendShutdownAsync();
            task.Wait();
            return task.Result;
        }

        public async Task<bool> SendShutdownAsync()
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var response = await client.PostAsync($"http://{Url}/shutdown", null);
                return (await response.Content.DeserialiseAsync<OperationStatus>()).ContainerStatus == ContainerStatus.ShuttingDown;
            }catch(Exception e)
            {
                _logger.LogError(e, "Failed to shutdown container, may become orphened");
                return false;
            }
        }

        public void ForceShutdown()
        {
            ForceShutdownAsync().Wait();
        }

        public async Task ForceShutdownAsync()
        {
            await _dockerClient.Containers.StopContainerAsync(Id, new ContainerStopParameters
            {
                WaitBeforeKillSeconds = 30
            }, CancellationToken.None);
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
                _logger.LogError(e, "Error getting container status");
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