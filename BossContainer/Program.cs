using PitBoss;
using Microsoft.AspNetCore.Hosting;

namespace BossContainer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BossWebServer server = new BossWebServer();
            var host = server.StartWebHost(args);
            host.WaitForShutdown();
        }
    }
}
