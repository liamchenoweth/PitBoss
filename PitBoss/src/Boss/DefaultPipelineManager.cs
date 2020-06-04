using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using PitBoss.Utils;
using System.Threading.Tasks;

namespace PitBoss {
    public class DefaultPipelineManager : IPipelineManager {
        // TODO: move this to config
        private const string CompileLocation = "compiled";
        private ILogger<IPipelineManager> _logger;
        private List<Pipeline> _pipelines;

        public DefaultPipelineManager(ILogger<IPipelineManager> logger) {
            _logger = logger;
        }

        public IEnumerable<Pipeline> Pipelines 
        { 
            get
            {
                return _pipelines;
            } 
        }

        public bool Ready {get; private set;} = false;

        public IEnumerable<Pipeline> CompilePipelines(string directory)
        {
            var task = CompilePipelinesAsync(directory);
            task.Wait();
            return task.Result;
        }

        public async Task<IEnumerable<Pipeline>> CompilePipelinesAsync(string directory)
        {
            _logger.LogInformation($"Begining pipeline compilation in {Path.GetFullPath(directory)}");
            if(!Directory.Exists(directory)) throw new DirectoryNotFoundException("Pipeline directory must exist.");
            var files = Directory.GetFiles(directory, "*.csx");
            _logger.LogInformation($"Found {files.Count()} scripts");
            var compiledFiles = new List<string>();
            foreach(var file in files) 
            {
                try
                {
                    _logger.LogInformation($"Compiling {file}");
                    compiledFiles.Add(await CompileDefinitionAsync(file));
                    _logger.LogInformation("Finished compilation");
                }
                catch(Exception e)
                {
                    _logger.LogError(e, $"Compilation failed, skipping failed file {file}");
                }
            }
            _logger.LogInformation($"Successfully compiled {compiledFiles.Count} scripts");

            var pipelines = new List<List<Pipeline>>();
            foreach(var file in compiledFiles)
            {
                try
                {
                    pipelines.Add(BuildDefinition(file));
                }
                catch(Exception e)
                {
                    _logger.LogError(e, $"Build failed, skipping failed file {file}");
                }
            }
            _pipelines = pipelines.SelectMany(i => i).ToList();
            _logger.LogInformation($"Successfully created {_pipelines.Count} pipelines");
            Ready = true;
            return _pipelines;
        }

        private async Task<string> CompileDefinitionAsync(string location) 
        {
            // Build our script to a dll so it can be imported
            await Compilation.CompileScriptAsync(location, CompileLocation);
            return $"{CompileLocation}/{Path.GetFileNameWithoutExtension(location)}.dll";
        }

        private List<Pipeline> BuildDefinition(string location) {
            // Load in our dll using the pipeline context
            var fullLoaction = Path.GetFullPath(location);
            var dir = Path.GetFullPath(Path.GetDirectoryName(location));
            PipelineLoadContext context = new PipelineLoadContext(dir);
            //var dll = context.LoadFromAssemblyPath(fullLoaction);
            var dll = Assembly.LoadFile(fullLoaction);

            // Get all types that we care about
            // Then create the pipelines from those types
            // Finally set the DLL location so we can send it off to the workers
            var types = dll.GetTypes().Where(x => x.GetInterfaces().Select(y => y.Name).Contains(typeof(IPipelineBuilder).Name));
            if(types.Count() == 0) throw new Exception($"No types that implement IPipelineBuilder found in {location}");
            var buidlers = types.Select(x => (IPipelineBuilder)Activator.CreateInstance(x)).Where(x => x != null).ToList();
            var pipelines = buidlers.Select(x => x.Build()).ToList();
            pipelines.ForEach(x => x.DllLocation = location);
            return pipelines;
        }

        public Pipeline GetPipeline(string name)
        {
            return _pipelines.FirstOrDefault(x => x.Name == name);
        }
    }
}