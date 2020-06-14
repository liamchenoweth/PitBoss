using System;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using System.Collections.Concurrent;

namespace PitBoss
{
    public class RedisDistributedService : IDistributedService
    {
        private IConfiguration _configuration;
        private ConnectionMultiplexer _redis;
        private IDistributedCache _cache;

        public RedisDistributedService(IConfiguration configuration)
        {
            _configuration = configuration;
            // TODO: Allow multiple redis connections for HA
            // TODO: Add extra options
            var redisOptions = new ConfigurationOptions {
                EndPoints = {{ _configuration["Boss:Cache:Redis:Host"], _configuration.GetValue<int>("Boss:Cache:Redis:Port") }},
                Password = _configuration["Boss:Cache:Redis:Password"]
            };
            _redis = ConnectionMultiplexer.Connect(redisOptions);
            _cache = new RedisCache(new RedisCacheOptions()
            {
                ConfigurationOptions = redisOptions
            });
        }

        public RedisDistributedService(
            ConnectionMultiplexer redis, 
            MemoryDistributedCache cache)
        {
            _redis = redis;
            _cache = cache;
        }

        public IDistributedQueue<T> GetQueue<T>(string queue)
        {
            return new RedisDistributedQueue<T>(_redis, queue);
        }

        public IDistributedQueue GetQueue(string queue, Type type)
        {
            var queueType = typeof(RedisDistributedQueue<>).MakeGenericType(new Type[] {type});
            return (IDistributedQueue) Activator.CreateInstance(queueType, new object[] { _redis, queue });
        }

        public IDistributedCache GetCache()
        {
            return _cache;
        }
    }
}