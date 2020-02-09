using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PitBoss;

namespace JobContainer
{
    public class Job
    {
        public static void Main(int port)
        {
            var host = StartJobServer(port);
            host.WaitForShutdown();
        }

        public static IWebHost StartJobServer(int port)
        {
            OperationWebServer server = new OperationWebServer();
            var host = server.StartWebHost(port);
            return host;
        }
    }
}
