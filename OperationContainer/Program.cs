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
    public class Program
    {
        public static void Main(string[] args)
        {
            OperationWebServer server = new OperationWebServer();
            var host = server.StartWebHost(args);
            host.WaitForShutdown();
        }
    }
}
