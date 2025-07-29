using System;
using System.Collections.Generic;
using Addressables.Core;

namespace Addressables.Pooling
{
    public class AddressablePool : IAddressablePool
    {
        private readonly PoolConfiguration _configuration;
        private readonly Dictionary<Type, Queue<object>> _pools;
        
        public AddressablePool(PoolConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _pools = new Dictionary<Type, Queue<object>>();
        }
        
        public bool TryGet<T>(out PooledAddressableHandle<T> handle) where T : class
        {
            var type = typeof(T);
            if (_pools.TryGetValue(type, out var pool) && pool.Count > 0)
            {
                handle = pool.Dequeue() as PooledAddressableHandle<T>;
                return handle != null;
            }
            
            handle = null;
            return false;
        }
        
        public void Return<T>(PooledAddressableHandle<T> handle) where T : class
        {
            if (handle == null) return;
            
            var type = typeof(T);
            if (!_pools.ContainsKey(type))
            {
                _pools[type] = new Queue<object>();
            }
            
            var pool = _pools[type];
            if (pool.Count < _configuration.MaxSize)
            {
                handle.Reset();
                pool.Enqueue(handle);
            }
        }
        
        public void Warmup()
        {
            // Pre-create pool objects if needed
            if (_configuration.WarmupOnStart)
            {
                // Implementation depends on your needs
            }
        }
        
        public void Clear()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }
            _pools.Clear();
        }
        
        public void Dispose()
        {
            Clear();
        }
    }
}