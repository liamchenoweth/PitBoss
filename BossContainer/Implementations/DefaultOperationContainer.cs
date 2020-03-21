using System;
using System.Net.Http;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Newtonsoft.Json;
using PitBoss.Extensions;
using JobContainer;

namespace PitBoss {
    public class DefaultOperationContainer : IOperationContainer {

        private IHttpClientFactory _clientFactory;
        private ILogger _logger;
        private IWebHost _host;
        private IConfiguration _configuration;

        public DefaultOperationContainer(PipelineStep step, IHttpClientFactory clientFactory, ILogger logger, IConfiguration configuration)
        {
            Operation = step.Name;
            _clientFactory = clientFactory;
            _logger = logger;
            _configuration = configuration;

            Id = Guid.NewGuid().ToString();
            Name = $"{Operation}-{Id}";
            
            _host = Job.StartJobServer(0);
            Url = _host.ServerFeatures.Get<IServerAddressesFeature>().Addresses.FirstOrDefault();
            
            var task = SendOperationAsync(step.Name);
            task.Wait();
        }

        public string Name { get; private set; }
        public string Url { get; private set; }
        public string Operation { get; private set; }
        public string Id { get; private set; }

        public async Task SendOperationAsync(string location)
        {
            var fileName = $"{_configuration["Boss:Scripts:Location"]}/{location}";
            var client = _clientFactory.CreateClient();
            MultipartFormDataContent content = new MultipartFormDataContent();
            byte[] payload = null;
            try
            {
                payload = await File.ReadAllBytesAsync(fileName);
            }catch(FileNotFoundException e)
            {
                _logger.LogCritical(e, $"Operation script not found at {fileName}, shutting down container");
                await _host.StopAsync();
            }
            content.Add(new ByteArrayContent(payload), "executionScript", Path.GetFileName(fileName));
            try
            {
                var response = await client.PostAsync($"{Url}/operation/script", content);
                response.EnsureSuccessStatusCode();
            }
            catch(Exception e)
            {
                _logger.LogCritical(e, "Was unable to send step to container, shutting down container");
                await _host.StopAsync();
            }
        }

        public void SendOperation(string location)
        {
            SendOperationAsync(location).RunSynchronously();
        }

        public void SendRequest(OperationRequest request)
        {
            SendRequestAsync(request).RunSynchronously();
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
            task.RunSynchronously();
            return task.Result;
        }

        public async Task<OperationStatus> HeartbeatAsync()
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync($"{Url}/heartbeat");
            return await response.Content.DeserialiseAsync<OperationStatus>();
        }

        public OperationStatus GetContainerStatus()
        {
            var task = GetContainerStatusAsync();
            task.RunSynchronously();
            return task.Result;
        }

        public async Task<OperationStatus> GetContainerStatusAsync()
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync($"{Url}/container/status");
            var output = await response.Content.ReadAsStringAsync();
            return await response.Content.DeserialiseAsync<OperationStatus>();
        }

        public OperationRequestStatus GetRequestStatus(OperationRequest request)
        {
            var task = GetRequestStatusAsync(request);
            task.RunSynchronously();
            return task.Result;
        }

        public async Task<OperationRequestStatus> GetRequestStatusAsync(OperationRequest request)
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync($"{Url}/operation/status?requestId={request.Id}");
            return await response.Content.DeserialiseAsync<OperationRequestStatus>();
        }

        public bool SendShutdown()
        {
            var task = SendShutdownAsync();
            task.RunSynchronously();
            return task.Result;
        }

        public async Task<bool> SendShutdownAsync()
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync($"{Url}/shutdown");
            return (await response.Content.DeserialiseAsync<OperationStatus>()).ContainerStatus == ContainerStatus.ShuttingDown;
        }

        public void ForceShutdown()
        {
            ForceShutdownAsync().RunSynchronously();
        }

        public async Task ForceShutdownAsync()
        {
            await _host.StopAsync();
        }

        public OperationDescription GetDescription()
        {
            var task = GetDescriptionAsync();
            task.RunSynchronously();
            return task.Result;
        }

        public async Task<OperationDescription> GetDescriptionAsync()
        {
            return new OperationDescription
            {
                Name = Name,
                Status = await GetContainerStatusAsync()
            };
        }
    }
}