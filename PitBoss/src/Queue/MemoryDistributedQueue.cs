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

        public MemoryDistributedQueue(ConcurrentQueue<string> queue)
        {
            _queue = queue;
        }

        public void Push(T obj)
        {
            _queue.Enqueue(JsonSerializer.Serialize(obj));
        }

        public void PushFront(T obj)
        {
            _queue = new ConcurrentQueue<string>(_queue.Prepend(JsonSerializer.Serialize(obj)));
        }

        public T Pop()
        {
            if(_queue.TryDequeue(out var obj)) return JsonSerializer.Deserialize<T>(obj);
            return default(T);
        }

        public T PopBack()
        {
            if(_queue.Count() == 0) return default(T);
            var obj = _queue.Last();
            _queue = new ConcurrentQueue<string>(_queue.Take(_queue.Count() - 1));
            return JsonSerializer.Deserialize<T>(obj);
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