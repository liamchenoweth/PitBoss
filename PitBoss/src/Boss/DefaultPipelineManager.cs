using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace PitBoss {
    public class DefaultPipelineManager : IPipelineManager {
        // TODO: move this to config
        private const string CompileLocation = "compiled";
        private ILogger _logger;
        private List<Pipeline> _pipelines;

        public DefaultPipelineManager(ILogger<IPipelineManager> logger) {
            _logger = logger;
        }

        public List<Pipeline> Pipelines 
        { 
            get
            {
                return _pipelines;
            } 
        }

        public List<Pipeline> CompilePipelines(string directory)
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
                    compiledFiles.Add(CompileDefinition(file));
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
            return _pipelines;
        }

        private string CompileDefinition(string location) 
        {
            // Build our script to a dll so it can be imported
            // Must be done on the command line as I can't find anywhere to call this in code
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo {
                    FileName = "dotnet",
                    Arguments = $"script publish {location} -o {CompileLocation} --dll -c Release",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };

            process.Start();
            process.WaitForExit();
            if(process.ExitCode != 0){
                throw new Exception($"Failed to compile {location}");
            }

            return $"{CompileLocation}/{Path.GetFileNameWithoutExtension(location)}.dll";
        }

        private List<Pipeline> BuildDefinition(string location) {
            // Load in our dll using the pipeline context
            var fullLoaction = Path.GetFullPath(location);
            var dir = Path.GetFullPath(Path.GetDirectoryName(location));
            PipelineLoadContext context = new PipelineLoadContext(dir);
            var dll = context.LoadFromAssemblyPath(fullLoaction);

            // Get all types that we care about
            // Then create the pipelines from those types
            // Finally set the DLL location so we can send it off to the workers
            var types = dll.GetTypes().Where(x => typeof(IPipelineBuilder).IsAssignableFrom(x));
            if(types.Count() == 0) throw new Exception($"No types that implement IPipelineBuilder found in {location}");
            var test = types.Select(x => Activator.CreateInstance(x)).ToList();
            var buidlers = types.Select(x => Activator.CreateInstance(x) as IPipelineBuilder).Where(x => x != null).ToList();
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