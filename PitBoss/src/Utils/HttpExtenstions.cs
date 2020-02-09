using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PitBoss.Extensions
{
    public static class HttpExtensions
    {
        public static async Task<T> DeserialiseAsync<T>(this HttpContent content)
        {
            return await JsonSerializer.DeserializeAsync<T>(await content.ReadAsStreamAsync());
        }

        public static T Deserialise<T>(this HttpContent content)
        {
            var task = content.DeserialiseAsync<T>();
            task.RunSynchronously();
            return task.Result;
        }
    }
    
    
}