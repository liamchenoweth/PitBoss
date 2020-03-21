using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PitBoss.Extensions
{
    public static class HttpExtensions
    {
        public static async Task<T> DeserialiseAsync<T>(this HttpContent content)
        {
            // Using newtonsoft here because dotnet core implementation doesn't work well with enums
            return await Task.Run(async () => JsonConvert.DeserializeObject<T>(await content.ReadAsStringAsync()));
        }

        public static T Deserialise<T>(this HttpContent content)
        {
            var task = content.DeserialiseAsync<T>();
            task.RunSynchronously();
            return task.Result;
        }
    }
    
    
}