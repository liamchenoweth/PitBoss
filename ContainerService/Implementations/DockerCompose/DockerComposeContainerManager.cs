using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Docker.DotNet;
using Docker.DotNet.Models;
using Newtonsoft.Json;

namespace PitBoss {
    public class DockerComposeContainerManager : IContainerManager {
        private DockerClient _docker;
        private Dictionary<string, IOperationGroup> _containers;
        private IHttpClientFactory _clientFactory;
        private ILogger _logger;
        private IConfiguration _configuration;

        public DockerComposeContainerManager(
            IHttpClientFactory clientFactory, 
            ILogger<DockerComposeContainerManager> logger,
            IConfiguration configuration) 
        {
            _containers = new Dictionary<string, IOperationGroup>();
            _clientFactory = clientFactory;
            _configuration = configuration;
            _logger = logger;
            _docker = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
        }

        public bool Ready {get; private set;} = false;

        public IEnumerable<IOperationGroup> GetContainers()
        {
            return _containers.Values;
        }

        public async Task<IEnumerable<IOperationGroup>> GetContainersAsync()
        {
            return _containers.Values;
        }

        public IOperationGroup GetContainersByStep(PipelineStep step)
        {
            var task = GetContainersByStepAsync(step);
            task.Wait();
            return task.Result;
        }

        public async Task<IOperationGroup> GetContainersByStepAsync(PipelineStep step)
        {
            if(_containers.TryGetValue(step.Name, out var group)) return group;
            throw new Exception($"Container group does not exist for {step.Name}");
        }

        public void SetGroupsStale()
        {
            foreach(var group in _containers)
            {
                group.Value.Stale = true;
            }
        }

        public void RegisterGroup(IOperationGroup group)
        {
            RegisterGroupAsync(group).Wait();
        }

        public async Task RegisterGroupAsync(IOperationGroup group)
        {
            if(_containers.ContainsKey(group.PipelineStep.Name))
            {
                _containers[group.PipelineStep.Name].Stale = false;
                return;
            }
            _containers.Add(group.PipelineStep.Name, group);
        }

        public void RemoveGroup(IOperationGroup group)
        {
            _containers.Remove(group.PipelineStep.Name);
        }

        public void DiscoverContainers() 
        {
            DiscoverContainersAsync().Wait();
        }

        public async Task DiscoverContainersAsync() 
        {
            var containers = (await _docker.Containers.ListContainersAsync(new ContainersListParameters{
                All = true
            }));
            foreach(var group in _containers)
            {
                var groupContainers = containers.Where(x => x.Labels.SingleOrDefault(x => x.Key == "group").Value == group.Key)
                .Where(x => x.State == "running")
                .Select(x => new DockerComposeOperationContainer(_clientFactory, _configuration, _logger, _docker)
                {
                    Url = x.Names.First().TrimStart('/'),
                    Name = x.Names.First().TrimStart('/'),
                    Operation = group.Key,
                    Id = x.Names.First().TrimStart('/')
                });
                await _containers[group.Key].SetContainersAsync(groupContainers);
            }
        }

        public void RunShutdown()
        {
            RunShutdownAsync().GetAwaiter().GetResult();
        }

        public async Task RunShutdownAsync()
        {
            foreach(var pair in _containers)
            {
                var group = pair.Value;
                await group.ShutdownGroupAsync();
            }
            _containers.Clear();
        }
    }
}