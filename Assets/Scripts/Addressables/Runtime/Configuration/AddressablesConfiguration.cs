using Addressables.Caching;
using Addressables.Loading;
using Addressables.Pooling;
using UnityEngine;

namespace Addressables.Configuration
{
    [CreateAssetMenu(fileName = "AddressablesConfiguration", menuName = "Addressables/Configuration")]
    public class AddressablesConfiguration : ScriptableObject
    {
        [Header("Loading Settings")]
        [Range(1, 10)]
        public int MaxConcurrentLoads = 3;
        
        [Header("Cache Settings")]
        [Range(10, 1000)]
        public int CacheSize = 100;
        
        public CachePolicy DefaultCachePolicy = CachePolicy.LRU;
        
        [Header("Pool Settings")]
        public PoolConfiguration PoolConfiguration = new PoolConfiguration();
        
        [Header("Warmup")]
        public string[] WarmupKeys;
        
        [Header("Analytics")]
        public bool EnableAnalytics = true;
        public bool LogPerformanceMetrics = false;
        
        [Header("Advanced")]
        public LoadingStrategy DefaultLoadingStrategy = LoadingStrategy.Lazy;
        public float AssetReleaseDelay = 5f;
    }
}