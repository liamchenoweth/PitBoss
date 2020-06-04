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
    
    
}