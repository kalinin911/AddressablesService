using System;
using Addressables.Core;

namespace Addressables.Pooling
{
    public interface IAddressablePool : IDisposable
    {
        bool TryGet<T>(out PooledAddressableHandle<T> handle) where T : class;
        void Return<T>(PooledAddressableHandle<T> handle) where T : class;
        void Warmup();
        void Clear();
    }
}