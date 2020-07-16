using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace PitBoss
{
    public class RedisDistributedQueue<T> : IDistributedQueue<T>
    {
        private ConnectionMultiplexer _redis;
        private string _name;

        public RedisDistributedQueue(ConnectionMultiplexer redis, string name)
        {
            _redis = redis;
            _name = name;
        }

        public void Push(T obj)
        {
            PushObject(obj);
        }

        public void PushObject(object obj)
        {
            var db = _redis.GetDatabase();
            db.ListRightPush(_name, JsonConvert.SerializeObject(obj));
        }

        public void PushFront(T obj)
        {
            var db = _redis.GetDatabase();
            db.ListLeftPush(_name, JsonConvert.SerializeObject(obj));
        }

        public T Pop()
        {
            var db = _redis.GetDatabase();
            var obj = db.ListLeftPop(_name);
            if(string.IsNullOrEmpty(obj)) return default;
            return JsonConvert.DeserializeObject<T>(obj);
        }

        public object PopObject()
        {
            return Pop();
        }

        public T PopBack()
        {
            var db = _redis.GetDatabase();
            var obj = db.ListRightPop(_name);
            return JsonConvert.DeserializeObject<T>(obj);
        }

        public IEnumerable<string> AllStrings
        {
            get
            {
                var db = _redis.GetDatabase();
                return db.ListRange(_name).Cast<string>();
            }
        }

        public IEnumerable<T> All 
        {
            get
            {
                return AllStrings.Select(x => JsonConvert.DeserializeObject<T>(x));
            }
        }
    }
}