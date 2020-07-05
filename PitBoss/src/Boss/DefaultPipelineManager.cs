using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using PitBoss.Utils;
using PitBoss.Extensions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PitBoss {
    public class DefaultPipelineManager : IPipelineManager {
        // TODO: move this to config
        private const string CompileLocation = "compiled";
        private ILogger<IPipelineManager> _logger;
        private List<Pipeline> _pipelines;
        private IBossContextFactory _contextFactory;
        private Dictionary<string, WeakReference> _references;

        public DefaultPipelineManager(
            ILogger<IPipelineManager> logger,
            IBossContextFactory context
        ) {
            _logger = logger;
            _contextFactory = context;
            _references = new Dictionary<string, WeakReference>();
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

        public void ClearPipelines()
        {
            _pipelines?.RemoveAll(x => true);
        }

        public async Task<IEnumerable<Pipeline>> CompilePipelinesAsync(string directory)
        {
            if(Directory.Exists(CompileLocation))
            {
                Directory.Delete(CompileLocation, true);
                Directory.CreateDirectory(CompileLocation);
            }
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
            _pipelines?.RemoveAll(x => true);
            var pipelines = new List<Pipeline>();
            foreach(var file in compiledFiles)
            {
                try
                {
                    // Clean up Assembly Load Context
                    if(_references.TryGetValue(file, out var reference))
                    {
                        for (int i = 0; reference.IsAlive && (i < 10); i++)
                        {
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            await Task.Delay(1000);
                        }
                        _references.Remove(file);
                    }
                    pipelines.AddRange(BuildDefinition(file, out var AlcWeakRef));
                    _references.Add(file, AlcWeakRef);
                }
                catch(Exception e)
                {
                    _logger.LogError(e, $"Build failed, skipping failed file {file}");
                }
            }
            _pipelines = pipelines;
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        private List<Pipeline> BuildDefinition(string location, out WeakReference reference) {
            // Load in our dll using the pipeline context
            var fullLoaction = Path.GetFullPath(location);
            var dir = Path.GetFullPath(Path.GetDirectoryName(location));
            //var dll = context.LoadFromAssemblyPath(fullLoaction);
            var context = new PipelineLoadContext(Directory.GetCurrentDirectory(), fullLoaction);
            reference = new WeakReference(context, trackResurrection: true);
            var dll = context.LoadFromAssemblyPath(fullLoaction);

            // Get all types that we care about
            // Then create the pipelines from those types
            // Finally set the DLL location so we can send it off to the workers
            //var types = dll.GetTypes().Where(x => x.GetInterfaces().Select(y => y.Name).Contains(typeof(IPipelineBuilder).Name));
            var types = dll.GetTypes().Where(x => typeof(IPipelineBuilder).IsAssignableFrom(x)).ToList();
            if(types.Count() == 0) throw new Exception($"No types that implement IPipelineBuilder found in {location}");
            var inter = types.Select(x => (IPipelineBuilder)Activator.CreateInstance(x)).ToList();
            var builders = inter.Where(x => x != null).ToList();
            var pipelines = builders.Select(x => x.Build()).ToList();
            pipelines.ForEach(x => x.DllLocation = location);
            context.Unload();
            return pipelines;
        }

        public Pipeline GetPipeline(string name)
        {
            return _pipelines.FirstOrDefault(x => x.Name == name);
        }

        public PipelineModel GetPipelineVersion(string version)
        {
            using(var context = _contextFactory.GetContext())
            {
                var pipeline = context.Pipelines.Include(x => x.Steps).ThenInclude(x => x.Step).FirstOrDefault(x => x.Version == version);
                pipeline.Steps = pipeline.Steps.OrderBy(x => x.Order).ToList();
                return pipeline;
            }
        }

        public void RegisterPipelines()
        {
            using(var context = _contextFactory.GetContext())
            {
                _logger.LogInformation("Regestering pipeline versions");
                var pipelineModels = _pipelines.Select(x => x.ToModel());
                var stepModels = _pipelines.SelectMany(x => x.Steps.Select(y => y.ToModel()));
                foreach(var model in stepModels)
                {
                    context.PipelineSteps.AddIfNotExists(x => x.HashCode == model.HashCode, model);
                }
                foreach(var pipeline in pipelineModels)
                {
                    var maps = pipeline.Steps;
                    pipeline.Steps = null;
                    context.Pipelines.AddIfNotExists(x => x.Version == pipeline.Version, pipeline);
                    for(var i = 0; i < maps.Count; i++)
                    {
                        var map = maps[i];
                        map.Version = map.Pipeline.Version;
                        map.StepHash = map.Step.HashCode;
                        map.Step = null;
                        map.Pipeline = null;
                        map.Order = i;
                        context.PipelineStepMap.AddIfNotExists(x => x.Version == map.Version && x.StepHash == map.StepHash, map);
                    }
                }
                context.SaveChanges();
            }
        }
    }
}