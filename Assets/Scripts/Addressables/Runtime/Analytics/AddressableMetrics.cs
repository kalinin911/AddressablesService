using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Addressables.Analytics
{
    public class AddressableMetrics : IAddressableAnalytics, IDisposable
    {
        private readonly Dictionary<string, List<float>> _loadTimes;
        private readonly Dictionary<string, Stopwatch> _activeTimers;
        private int _cacheHits;
        private int _cacheMisses;
        private int _successfulLoads;
        private int _failedLoads;
        
        public AddressableMetrics()
        {
            _loadTimes = new Dictionary<string, List<float>>();
            _activeTimers = new Dictionary<string, Stopwatch>();
        }
        
        public void RecordLoadTime(string key, float timeMs)
        {
            if (!_loadTimes.ContainsKey(key))
                _loadTimes[key] = new List<float>();
            _loadTimes[key].Add(timeMs);
        }
        
        public void RecordCacheHit(string key)
        {
            _cacheHits++;
        }
        
        public void RecordCacheMiss(string key)
        {
            _cacheMisses++;
        }
        
        public void RecordSuccessfulLoad(string key)
        {
            _successfulLoads++;
        }
        
        public void RecordFailedLoad(string key, Exception exception)
        {
            _failedLoads++;
            UnityEngine.Debug.LogError($"Failed to load {key}: {exception.Message}");
        }
        
        public AddressableMetricsData GetMetrics()
        {
            return new AddressableMetricsData
            {
                CacheHitRate = CalculateCacheHitRate(),
                AverageLoadTime = CalculateAverageLoadTime(),
                TotalLoads = _successfulLoads,
                FailedLoads = _failedLoads,
                LoadTimesByKey = GetAverageLoadTimesByKey()
            };
        }
        
        public IDisposable MeasureLoadTime(string key)
        {
            var stopwatch = Stopwatch.StartNew();
            _activeTimers[key] = stopwatch;
            
            return new LoadTimeMeasurement(this, key, stopwatch);
        }
        
        private float CalculateCacheHitRate()
        {
            var total = _cacheHits + _cacheMisses;
            return total > 0 ? (float)_cacheHits / total : 0f;
        }
        
        private float CalculateAverageLoadTime()
        {
            if (_loadTimes.Count == 0) return 0f;
            
            var allTimes = _loadTimes.Values.SelectMany(x => x).ToList();
            return allTimes.Count > 0 ? allTimes.Average() : 0f;
        }
        
        private Dictionary<string, float> GetAverageLoadTimesByKey()
        {
            var result = new Dictionary<string, float>();
            foreach (var kvp in _loadTimes)
            {
                if (kvp.Value.Count > 0)
                {
                    result[kvp.Key] = kvp.Value.Average();
                }
            }
            return result;
        }
        
        public void Dispose()
        {
            _loadTimes.Clear();
            _activeTimers.Clear();
        }
        
        private class LoadTimeMeasurement : IDisposable
        {
            private readonly AddressableMetrics _metrics;
            private readonly string _key;
            private readonly Stopwatch _stopwatch;
            
            public LoadTimeMeasurement(AddressableMetrics metrics, string key, Stopwatch stopwatch)
            {
                _metrics = metrics;
                _key = key;
                _stopwatch = stopwatch;
            }
            
            public void Dispose()
            {
                _stopwatch.Stop();
                _metrics.RecordLoadTime(_key, (float)_stopwatch.ElapsedMilliseconds);
                _metrics._activeTimers.Remove(_key);
            }
        }
    }
}