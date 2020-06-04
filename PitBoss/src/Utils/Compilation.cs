using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Serilog;

namespace PitBoss.Utils
{

    public class ProcessExit
    {
        public int StatusCode;
        public string StdOut;
        public string StdErr;
    }

    public static class Compilation
    {
        public static async Task CompileScriptAsync(string location, string outLocation)
        {
            var tcs = new TaskCompletionSource<ProcessExit>();

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

            process.EnableRaisingEvents = true;
            process.Exited += async (sender, args) =>
            {  
                var exit = new ProcessExit
                {
                    StatusCode = process.ExitCode,
                    StdOut = await process.StandardOutput.ReadToEndAsync(),
                    StdErr = await process.StandardError.ReadToEndAsync()
                };
                tcs.SetResult(exit);
                process.Dispose();
            };

            process.Start();
            var output = await tcs.Task;
            if(output.StatusCode != 0){
                throw new Exception($"Failed to compile {location}\nErr:\n{output.StdErr}\nOut:\n{output.StdOut}");
            }
        }

        public static void CompileScript(string location, string outLocation) => CompileScriptAsync(location, outLocation).Wait();
    }
}