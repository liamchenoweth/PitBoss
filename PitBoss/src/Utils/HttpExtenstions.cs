using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace PitBoss.Extensions
{
    public static class HttpExtensions
    {
        public static async Task<T> DeserialiseAsync<T>(this HttpContent content)
        {
            // Using newtonsoft here because dotnet core implementation doesn't work well with enums
            var stringContent = await content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(stringContent);
        }

        public static T Deserialise<T>(this HttpContent content)
        {
            var task = content.DeserialiseAsync<T>();
            task.Wait();
            return task.Result;
        }
    }

    public class NoiseFilter : IHttpMessageHandlerBuilderFilter
    {
        private readonly ILoggerFactory _loggerFactory;

        public NoiseFilter(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            return (builder) =>
            {
                // Run other configuration first, we want to decorate.
                next(builder);
                
                // Lets not Log HttpClient Output right now
                // TODO: Put some actual logging here that isn't as noisy as the stock impl
            };
        }
    }
    
    
}