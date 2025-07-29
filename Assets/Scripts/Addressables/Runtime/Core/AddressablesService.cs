using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Addressables.Analytics;
using Addressables.Caching;
using Addressables.Configuration;
using Addressables.Loading;
using Addressables.Pooling;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Addressables.Core
{
    public sealed class AddressablesService : IAddressablesService
    {
        private readonly IAddressableCache _cache;
        private readonly ILoadingStrategy _loadingStrategy;
        private readonly IAddressablePool _pool;
        private readonly AddressablesConfiguration _configuration;
        private readonly Dictionary<string, AsyncOperationHandle> _activeHandles;
        private readonly SemaphoreSlim _loadSemaphore;
        private readonly AddressableMetrics _metrics;
        
        public event Action<string, float> OnLoadProgress;
        public event Action<string, object> OnAssetLoaded;
        public event Action<string, Exception> OnLoadFailed;
        
        public IAddressableAnalytics Analytics => _metrics;
        
        public AddressablesService(
            AddressablesConfiguration configuration,
            IAddressableCache cache = null,
            ILoadingStrategy loadingStrategy = null,
            IAddressablePool pool = null)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _cache = cache ?? new LRUAddressableCache(configuration.CacheSize);
            _loadingStrategy = loadingStrategy ?? new LazyLoadStrategy();
            _pool = pool ?? new AddressablePool(configuration.PoolConfiguration);
            _activeHandles = new Dictionary<string, AsyncOperationHandle>();
            _loadSemaphore = new SemaphoreSlim(configuration.MaxConcurrentLoads);
            _metrics = new AddressableMetrics();
        }
        
        public async UniTask<T> LoadAssetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
                
            // Check cache first
            if (_cache.TryGet(key, out T cachedAsset))
            {
                _metrics.RecordCacheHit(key);
                return cachedAsset;
            }
            
            _metrics.RecordCacheMiss(key);
            
            await _loadSemaphore.WaitAsync(cancellationToken);
            try
            {
                using var performanceScope = _metrics.MeasureLoadTime(key);
                
                // Check if already loading
                if (_activeHandles.TryGetValue(key, out var existingHandle))
                {
                    return await ConvertHandleAsync<T>(existingHandle, cancellationToken);
                }
                
                // Load with strategy
                var handle = await _loadingStrategy.LoadAsync<T>(key, progress =>
                {
                    OnLoadProgress?.Invoke(key, progress);
                }, cancellationToken);
                
                _activeHandles[key] = handle;
                
                var result = handle.Result as T;
                if (result != null)
                {
                    _cache.Add(key, result);
                    OnAssetLoaded?.Invoke(key, result);
                    _metrics.RecordSuccessfulLoad(key);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _metrics.RecordFailedLoad(key, ex);
                OnLoadFailed?.Invoke(key, ex);
                throw new AddressableLoadException($"Failed to load asset: {key}", ex);
            }
            finally
            {
                _loadSemaphore.Release();
            }
        }
        
        public async UniTask<T> LoadAssetAsync<T>(AssetReference reference, CancellationToken cancellationToken = default) where T : class
        {
            if (reference == null)
                throw new ArgumentNullException(nameof(reference));
                
            return await LoadAssetAsync<T>(reference.AssetGUID, cancellationToken);
        }
        
        public async UniTask<IAddressableHandle<T>> LoadWithHandleAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            var asset = await LoadAssetAsync<T>(key, cancellationToken);
            
            // Try to get from pool first
            if (_pool.TryGet<T>(out var pooledHandle))
            {
                pooledHandle.Initialize(asset, key, this);
                return pooledHandle;
            }
            
            return new AddressableHandle<T>(asset, key, this);
        }
        
        public void ReleaseAsset<T>(T asset) where T : class
        {
            if (asset == null) return;
            
            var key = _cache.GetKey(asset);
            if (!string.IsNullOrEmpty(key))
            {
                ReleaseByKey(key);
            }
        }
        
        public void ReleaseHandle<T>(IAddressableHandle<T> handle) where T : class
        {
            if (handle == null) return;
            
            // Return to pool if possible
            if (handle is PooledAddressableHandle<T> pooledHandle)
            {
                _pool.Return(pooledHandle);
            }
            else
            {
                ReleaseByKey(handle.Key);
            }
        }
        
        public async UniTask PreloadAssetsAsync(string[] keys, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            if (keys == null || keys.Length == 0) return;
            
            var tasks = keys.Select(async (key, index) =>
            {
                try
                {
                    await LoadAssetAsync<object>(key, cancellationToken);
                    progress?.Report((index + 1) / (float)keys.Length);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to preload {key}: {ex.Message}");
                }
            });
            
            await UniTask.WhenAll(tasks);
        }
        
        public async UniTask WarmupAsync(CancellationToken cancellationToken = default)
        {
            if (_configuration.WarmupKeys?.Length > 0)
            {
                await PreloadAssetsAsync(_configuration.WarmupKeys, null, cancellationToken);
            }
            
            _pool.Warmup();
        }
        
        public bool TryGetCached<T>(string key, out T asset) where T : class
        {
            return _cache.TryGet(key, out asset);
        }
        
        public void ClearCache()
        {
            _cache.Clear();
            
            // Release all active handles
            foreach (var handle in _activeHandles.Values)
            {
                if (handle.IsValid())
                {
                    UnityEngine.AddressableAssets.Addressables.Release(handle);
                }
            }
            
            _activeHandles.Clear();
        }
        
        public void Dispose()
        {
            ClearCache();
            _loadSemaphore?.Dispose();
            _pool?.Dispose();
            _metrics?.Dispose();
        }
        
        private void ReleaseByKey(string key)
        {
            if (_activeHandles.TryGetValue(key, out var handle))
            {
                if (handle.IsValid())
                {
                    UnityEngine.AddressableAssets.Addressables.Release(handle);
                }
                _activeHandles.Remove(key);
            }
            
            _cache.Remove(key);
        }
        
        private async UniTask<T> ConvertHandleAsync<T>(AsyncOperationHandle handle, CancellationToken cancellationToken) where T : class
        {
            while (!handle.IsDone && !cancellationToken.IsCancellationRequested)
            {
                await UniTask.Yield();
            }
            
            cancellationToken.ThrowIfCancellationRequested();
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return handle.Result as T;
            }
            
            throw new AddressableLoadException($"Handle conversion failed: {handle.OperationException?.Message}");
        }
    }
}