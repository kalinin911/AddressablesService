using System;
using System.Collections.Generic;
using System.Linq;

namespace Addressables.Caching
{
    public class LRUAddressableCache : IAddressableCache
    {
        private readonly int _maxSize;
        private readonly Dictionary<string, CacheEntry> _cache;
        private readonly LinkedList<string> _lruList;
        private readonly Dictionary<object, string> _reverseMap;
        private readonly object _lock = new object();
        
        private class CacheEntry
        {
            public object Asset { get; set; }
            public LinkedListNode<string> Node { get; set; }
            public DateTime LastAccessed { get; set; }
            public int HitCount { get; set; }
        }
        
        public LRUAddressableCache(int maxSize)
        {
            _maxSize = maxSize;
            _cache = new Dictionary<string, CacheEntry>(maxSize);
            _lruList = new LinkedList<string>();
            _reverseMap = new Dictionary<object, string>();
        }
        
        public bool TryGet<T>(string key, out T asset) where T : class
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var entry))
                {
                    // Move to front (most recently used)
                    _lruList.Remove(entry.Node);
                    entry.Node = _lruList.AddFirst(key);
                    entry.LastAccessed = DateTime.UtcNow;
                    entry.HitCount++;
                    
                    asset = entry.Asset as T;
                    return asset != null;
                }
                
                asset = null;
                return false;
            }
        }
        
        public void Add<T>(string key, T asset) where T : class
        {
            if (asset == null) return;
            
            lock (_lock)
            {
                if (_cache.ContainsKey(key))
                {
                    // Update existing
                    var entry = _cache[key];
                    _reverseMap.Remove(entry.Asset);
                    entry.Asset = asset;
                    _reverseMap[asset] = key;
                    
                    // Move to front
                    _lruList.Remove(entry.Node);
                    entry.Node = _lruList.AddFirst(key);
                    entry.LastAccessed = DateTime.UtcNow;
                }
                else
                {
                    // Evict if necessary
                    if (_cache.Count >= _maxSize)
                    {
                        EvictLeastRecentlyUsed();
                    }
                    
                    // Add new
                    var node = _lruList.AddFirst(key);
                    _cache[key] = new CacheEntry
                    {
                        Asset = asset,
                        Node = node,
                        LastAccessed = DateTime.UtcNow,
                        HitCount = 0
                    };
                    _reverseMap[asset] = key;
                }
            }
        }
        
        public void Remove(string key)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var entry))
                {
                    _lruList.Remove(entry.Node);
                    _cache.Remove(key);
                    _reverseMap.Remove(entry.Asset);
                }
            }
        }
        
        public void Clear()
        {
            lock (_lock)
            {
                _cache.Clear();
                _lruList.Clear();
                _reverseMap.Clear();
            }
        }
        
        public string GetKey(object asset)
        {
            lock (_lock)
            {
                return _reverseMap.TryGetValue(asset, out var key) ? key : null;
            }
        }
        
        public CacheStatistics GetStatistics()
        {
            lock (_lock)
            {
                return new CacheStatistics
                {
                    TotalEntries = _cache.Count,
                    MaxSize = _maxSize,
                    HitRate = CalculateHitRate(),
                    MostAccessedKeys = _cache.OrderByDescending(x => x.Value.HitCount)
                        .Take(10)
                        .Select(x => x.Key)
                        .ToList()
                };
            }
        }
        
        private void EvictLeastRecentlyUsed()
        {
            var lastKey = _lruList.Last?.Value;
            if (lastKey != null)
            {
                Remove(lastKey);
            }
        }
        
        private float CalculateHitRate()
        {
            var totalHits = _cache.Values.Sum(x => x.HitCount);
            var totalRequests = totalHits + _cache.Count;
            return totalRequests > 0 ? (float)totalHits / totalRequests : 0f;
        }
    }
}