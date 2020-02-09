using System.Collections.Generic;
using Microsoft.Extensions.Caching.Distributed;

namespace PitBoss
{
    public interface IDistributedService
    {
        IDistributedQueue<T> GetQueue<T>(string queue);
        IDistributedCache GetCache();
    }

    public interface IDistributedQueue<T>
    {
        void Push(T value);
        void PushFront(T value);
        T Pop();
        T PopBack();
        IEnumerable<T> All { get; }
    }
}