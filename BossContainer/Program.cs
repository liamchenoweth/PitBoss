using PitBoss;
using Microsoft.AspNetCore.Hosting;

namespace BossContainer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BossWebServer.CreateHostBuilder(args).Build().Run();
        }
    }
}
