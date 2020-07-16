using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PitBoss
{
    public class MemoryDistributedQueue<T> : IDistributedQueue<T>
    {
        private ConcurrentQueue<string> _queue;

        public MemoryDistributedQueue(ref ConcurrentQueue<string> queue)
        {
            _queue = queue;
        }

        public void Push(T obj)
        {
            PushObject(obj);
        }

        public void PushObject(object obj)
        {
            _queue.Enqueue(JsonConvert.SerializeObject(obj));
        }

        public void PushFront(T obj)
        {
            _queue.Enqueue(JsonConvert.SerializeObject(obj));
        }

        public T Pop()
        {
            if(_queue.TryDequeue(out var obj)) return JsonConvert.DeserializeObject<T>(obj);
            return default(T);
        }

        public object PopObject()
        {
            return Pop();
        }

        public T PopBack()
        {
            throw new NotImplementedException("Current implementation does not allow this");
        }

        public IEnumerable<string> AllStrings
        {
            get
            {
                return _queue;
            }
        }

        public IEnumerable<T> All 
        {
            get
            {
                return _queue.Select(x => JsonConvert.DeserializeObject<T>(x));
            }
        }
    }
}