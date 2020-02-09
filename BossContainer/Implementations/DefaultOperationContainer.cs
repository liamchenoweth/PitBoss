using System;
using System.Net.Http;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using PitBoss.Extensions;
using JobContainer;

namespace PitBoss {
    public class DefaultOperationContainer : IOperationContainer {

        private IHttpClientFactory _clientFactory;
        private IWebHost _host;

        public DefaultOperationContainer(PipelineStep step, IHttpClientFactory clientFactory)
        {
            Operation = step.Name;
            _clientFactory = clientFactory;

            Id = Guid.NewGuid().ToString();
            Name = $"{Operation}-{Id}";
            
            Job.StartJobServer(0);
            var port = _host.ServerFeatures.Get<IServerAddressesFeature>().Addresses.FirstOrDefault();
            Url = $"localhost:{port}";
        }

        public string Name { get; private set; }
        public string Url { get; private set; }
        public string Operation { get; private set; }
        public string Id { get; private set; }

        public async Task SendOperationAsync(string location)
        {
            var client = _clientFactory.CreateClient();
            MultipartFormDataContent content = new MultipartFormDataContent();
            var payload = await File.ReadAllBytesAsync(location);
            content.Add(new ByteArrayContent(payload), "executionScript", Path.GetFileName(location));
            var response = await client.PostAsync($"{Url}/operation/script", content);
            response.EnsureSuccessStatusCode();
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
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(JsonSerializer.Serialize<OperationRequest>(request)), "request");
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

        public OperationStatus GetStatus()
        {
            var task = GetStatusAsync();
            task.RunSynchronously();
            return task.Result;
        }

        public async Task<OperationStatus> GetStatusAsync()
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync($"{Url}/operation/status");
            return await response.Content.DeserialiseAsync<OperationStatus>();
        }

        public OperationStatus GetStatus(OperationRequest request)
        {
            var task = GetStatusAsync(request);
            task.RunSynchronously();
            return task.Result;
        }

        public async Task<OperationStatus> GetStatusAsync(OperationRequest request)
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync($"{Url}/operation/status?requestId={request.Id}");
            return await response.Content.DeserialiseAsync<OperationStatus>();
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
    }
}