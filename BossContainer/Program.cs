using PitBoss;
using Microsoft.AspNetCore.Hosting;

namespace BossContainer
{
    public class Boss
    {
        public static void Main(string[] args)
        {
            BossWebServer server = new BossWebServer();
            server.UseContainerManager<DefaultContainerManager>();
            server.UseContainerBalancer<DefaultContainerBalancer>();
            var host = server.StartWebHost(args);
            host.WaitForShutdown();
        }
    }
}
