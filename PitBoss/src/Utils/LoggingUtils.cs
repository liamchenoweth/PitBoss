using Serilog;

namespace PitBoss.Utils {
    public static class LoggingUtils 
    {
        public static ILogger ConfigureSerilog()
        {
            return new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
        }  
    }
}