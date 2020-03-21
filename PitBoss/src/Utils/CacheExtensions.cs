using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace PitBoss.Extensions 
{
    public static class CacheExtensions
    {
        // TODO: async functions
        public static void Set<T>(this IDistributedCache cache, string key, T obj)
        {
            var objString = JsonConvert.SerializeObject(obj);
            cache.SetString(key, objString);
        }

        public static T Get<T>(this IDistributedCache cache, string key)
        {
            var objString = cache.GetString(key);
            return JsonConvert.DeserializeObject<T>(objString);
        }
    }
}