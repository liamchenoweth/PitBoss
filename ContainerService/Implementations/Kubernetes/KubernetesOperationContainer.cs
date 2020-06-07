using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Rest;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PitBoss.Utils;
using PitBoss.Extensions;
using k8s;
using k8s.Models;

namespace PitBoss
{
    public class KubernetesOperationContainer : IOperationContainer
    {
        private IHttpClientFactory _clientFactory;
        private IConfiguration _configuration;
        private ILogger _logger;
        private IKubernetes _kubernetesClient;
        private PipelineStep _step;

        public string Name { get; set; }
        public string Url { get; set; }
        public string Operation { get; set; }
        public string Id { get; set; }

        public KubernetesOperationContainer(
            PipelineStep step,
            IHttpClientFactory clientFactory, 
            IConfiguration configuration, 
            ILogger logger,
            IKubernetes client)
        {
            _kubernetesClient = client;
            _clientFactory = clientFactory;
            _configuration = configuration;
            _logger = logger;
            _step = step;

            Name = FileUtils.RandomString(8).ToLower();
            Operation = step.Name;

            var definition = new V1Pod()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = Name,
                    Labels = new Dictionary<string, string>
                    {
                        {"group", step.Name}
                    },
                    NamespaceProperty = _configuration["ContainerService:Containers:Namespace"]
                },
                Spec = new V1PodSpec()
                {
                    Containers = new List<V1Container>
                    {
                        new V1Container()
                        {
                            Ports = new List<V1ContainerPort>()
                            {
                                new V1ContainerPort(80)
                            },
                            Name = "worker",
                            // LivenessProbe = new V1Probe()
                            // {
                            //     HttpGet = new V1HTTPGetAction(80, path: "container/status")
                            // },
                            Image = _configuration["Operations:Image"],
                            // VolumeMounts = new List<V1VolumeMount>()
                            // {
                            //     new V1VolumeMount("/app/OperationContainer", "empty-dir", readOnlyProperty: false)
                            // },
                            ImagePullPolicy = "IfNotPresent"
                        }
                    },
                    // Volumes = new List<V1Volume>()
                    // {
                    //     new V1Volume("empty-dir", emptyDir: new V1EmptyDirVolumeSource())
                    // }
                }
            };
            
            V1Pod pod = null;
            try
            {
                definition.Validate();
                pod = client.CreateNamespacedPod(definition, _configuration["ContainerService:Containers:Namespace"]);
            }
            catch(HttpOperationException e)
            {
                _logger.LogError(e, $"Failed to send pod to master, failed with response {e.Response.Content}");
                throw e;
            }
            catch(Exception e)
            {
                _logger.LogError(e, $"Failed to create Pod with definition: \n {JsonConvert.SerializeObject(definition)}");
                throw e;
            }
            Id = pod.Metadata.Uid;
            Task.Delay(10000).Wait();
            Url = client.ListNamespacedPod(_configuration["ContainerService:Containers:Namespace"]).Items.Single(x => x.Name() == Name).Status.PodIP;
            Exception exception = null;
            for(var x = 0; x < 4; x++)
            {
                try
                {
                    _logger.LogInformation($"Calling to http://{Url}/container/status");
                    _clientFactory.CreateClient().GetAsync($"http://{Url}/container/status").Wait();
                    return;
                }
                catch(Exception e)
                {
                    exception = e;
                    _logger.LogInformation($"Waiting for {Name} to be come ready");
                    Task.Delay(10000).Wait();
                }
            }
            _logger.LogError(exception, "Container did not become ready forcing container shutdown");
            ForceShutdown();
            throw new Exception("Container did not become ready");
        }

        public KubernetesOperationContainer(
            IHttpClientFactory clientFactory, 
            IConfiguration configuration, 
            ILogger logger, 
            IKubernetes client)
        {
            _kubernetesClient = client;
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
            var client = _clientFactory.CreateClient();
            var response = await client.PostAsync($"http://{Url}/shutdown", null);
            return (await response.Content.DeserialiseAsync<OperationStatus>()).ContainerStatus == ContainerStatus.ShuttingDown;
        }

        public void ForceShutdown()
        {
            ForceShutdownAsync().Wait();
        }

        public async Task ForceShutdownAsync()
        {
            await _kubernetesClient.DeleteNamespacedPodAsync(Name, _configuration["ContainerService:Containers:Namespace"]);
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