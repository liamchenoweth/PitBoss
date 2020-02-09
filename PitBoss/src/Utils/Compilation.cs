using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PitBoss.Utils
{

    public static class Compilation
    {
        public static async Task CompileScriptAsync(string location, string outLocation)
        {
            var tcs = new TaskCompletionSource<int>();

            // Must be done on the command line as I can't find anywhere to call this in code
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo {
                    FileName = "dotnet",
                    Arguments = $"script publish {location} -o {outLocation} --dll -c Release",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();
            await tcs.Task;
            if(process.ExitCode != 0){
                throw new Exception($"Failed to compile {location}");
            }
        }

        public static void CompileScript(string location, string outLocation) => CompileScriptAsync(location, outLocation).RunSynchronously();
    }
}