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

namespace PitBoss {
    public class HttpContainer : IOperationContainer {

        private IHttpClientFactory _clientFactory;
        private ILogger _logger;
        private IConfiguration _configuration;

        public HttpContainer(
            PipelineStep step, 
            IHttpClientFactory clientFactory, 
            ILogger logger, 
            IConfiguration configuration,
            OperationDescription description)
        {
            Operation = step.Name;
            _clientFactory = clientFactory;
            _logger = logger;
            _configuration = configuration;

            Id = description.Id;
            Name = description.Name;
            Url = description.Url;
        }

        public string Name { get; private set; }
        public string Url { get; private set; }
        public string Operation { get; private set; }
        public string Id { get; private set; }

        public async Task SendOperationAsync(string location)
        {
            throw new Exception("This type of operation is not available on HttpContainer");
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
            await client.PostAsync($"http://{Url}/operation/request", content);
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
            throw new Exception("This type of operation is not available on HttpContainer");
        }

        public void ForceShutdown()
        {
            ForceShutdownAsync().Wait();
        }

        public async Task ForceShutdownAsync()
        {
            throw new Exception("This type of operation is not available on HttpContainer");
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