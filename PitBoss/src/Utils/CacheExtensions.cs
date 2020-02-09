using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace PitBoss.Extensions 
{
    public static class CacheExtensions
    {
        // TODO: async functions
        public static void Set<T>(this IDistributedCache cache, string key, T obj)
        {
            cache.Set(key, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj)));
        }

        public static T Get<T>(this IDistributedCache cache, string key)
        {
            var bytes = cache.Get(key);
            return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(bytes));
        }
    }
}