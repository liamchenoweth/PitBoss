using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace PitBoss
{
    public class MemoryDistributedService : IDistributedService
    {
        private ConcurrentDictionary<string, ConcurrentQueue<string>> _queues;
        private IDistributedCache _cache;

        public MemoryDistributedService()
        {
            _queues = new ConcurrentDictionary<string, ConcurrentQueue<string>>();
            _cache = new MemoryDistributedCache(new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions()));
        }

        public IDistributedQueue<T> GetQueue<T>(string queue)
        {
            if(!_queues.ContainsKey(queue)) _queues[queue] = new ConcurrentQueue<string>();
            var queueRef = _queues[queue];
            return new MemoryDistributedQueue<T>(ref queueRef);
        }

        public IDistributedQueue GetQueue(string queue, Type type)
        {
            var queueType = typeof(MemoryDistributedQueue<>).MakeGenericType(new Type[] {type});
            if(!_queues.ContainsKey(queue)) _queues[queue] = new ConcurrentQueue<string>();
            var queueRef = _queues[queue];
            return (IDistributedQueue) Activator.CreateInstance(queueType, new object[] { queueRef });
        }

        public IDistributedCache GetCache()
        {
            return _cache;
        }
    }
}