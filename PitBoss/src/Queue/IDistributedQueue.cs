using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Distributed;

namespace PitBoss
{
    public interface IDistributedService
    {
        IDistributedQueue<T> GetQueue<T>(string queue);
        IDistributedQueue GetQueue(string queue, Type type);
        IDistributedCache GetCache();
    }

    public interface IDistributedQueue
    {
        void PushObject(object value);
        object PopObject();
        IEnumerable<string> AllStrings { get; }
    }

    public interface IDistributedQueue<T> : IDistributedQueue
    {
        void Push(T value);
        void PushFront(T value);
        T Pop();
        T PopBack();
        IEnumerable<T> All { get; }
    }
}