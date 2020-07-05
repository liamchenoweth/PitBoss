using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace PitBoss {
    public class PipelineLoadContext : AssemblyLoadContext 
    {
        private AssemblyDependencyResolver _pitbossResolver;
        private AssemblyDependencyResolver _pluginResolver;

        public PipelineLoadContext(string pitbossPath, string pluginPath) : base(isCollectible: true)
        {
            _pitbossResolver = new AssemblyDependencyResolver(pitbossPath);
            _pluginResolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            var assembly = Default.Assemblies.FirstOrDefault(x => x.FullName == assemblyName.FullName);
            if (assembly != null)
            {
                return assembly;
            }
            string assemblyPath = _pluginResolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }
            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libraryPath = _pluginResolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }
}