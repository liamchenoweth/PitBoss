using System;
using System.Linq;
using System.Text.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;

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
            _queue.Enqueue(JsonSerializer.Serialize(obj));
        }

        public void PushFront(T obj)
        {
            
            _queue.Enqueue(JsonSerializer.Serialize(obj));
        }

        public T Pop()
        {
            if(_queue.TryDequeue(out var obj)) return JsonSerializer.Deserialize<T>(obj);
            return default(T);
        }

        public T PopBack()
        {
            throw new NotImplementedException("Current implementation does not allow this");
        }

        public IEnumerable<T> All 
        {
            get
            {
                return _queue.Select(x => JsonSerializer.Deserialize<T>(x));
            }
        }
    }
}