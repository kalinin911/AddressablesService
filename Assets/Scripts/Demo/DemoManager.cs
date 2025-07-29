using System;
using Addressables.Configuration;
using Addressables.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Demo
{
    public class DemoManager : MonoBehaviour
    {
        [SerializeField] private AddressablesConfiguration configuration;
        private IAddressablesService _addressablesService;
    
        private async UniTaskVoid Start()
        {
            // Load configuration from Resources if not assigned
            if (configuration == null)
            {
                configuration = Resources.Load<AddressablesConfiguration>("AddressablesConfig");
                if (configuration == null)
                {
                    Debug.LogError("AddressablesConfiguration not found in Resources!");
                    return;
                }
            }
        
            // Initialize service
            _addressablesService = new Addressables.Core.AddressablesService(configuration);
        
            // Subscribe to events
            //_addressablesService.OnLoadProgress += (key, progress) => 
                //Debug.Log($"Loading {key}: {progress:P}");
            
            _addressablesService.OnAssetLoaded += (key, asset) => 
                Debug.Log($"Loaded: {key}");
            
            _addressablesService.OnLoadFailed += (key, exception) => 
                Debug.LogError($"Failed to load {key}: {exception.Message}");
        
            // Warmup
            Debug.Log("Starting warmup...");
            await _addressablesService.WarmupAsync();
            Debug.Log("Warmup complete!");
        
            // Demo loading
            await DemoLoadingAsync();
        }
    
        private async UniTask DemoLoadingAsync()
        {
            try
            {
                // Example 1: Simple asset loading
                Debug.Log("=== Example 1: Simple Loading ===");
                var cubePrefab = await _addressablesService.LoadAssetAsync<GameObject>("Cube");
                if (cubePrefab != null)
                {
                    var cube = Instantiate(cubePrefab, new Vector3(-3, 0, 0), Quaternion.identity);
                    cube.name = "Loaded Cube";
                }
            
                // Example 2: Loading with handle
                Debug.Log("=== Example 2: Handle Loading ===");
                using (var sphereHandle = await _addressablesService.LoadWithHandleAsync<GameObject>("Sphere"))
                {
                    if (sphereHandle.Asset != null)
                    {
                        var sphere = Instantiate(sphereHandle.Asset, Vector3.zero, Quaternion.identity);
                        sphere.name = "Loaded Sphere (with handle)";
                    }
                    // Handle will be released when leaving scope
                }
            
                // Example 3: Demonstrate caching
                Debug.Log("=== Example 3: Cache Demo ===");
                Debug.Log("Loading Capsule first time...");
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var capsule1 = await _addressablesService.LoadAssetAsync<GameObject>("Capsule");
                stopwatch.Stop();
                Debug.Log($"First load took: {stopwatch.ElapsedMilliseconds}ms");
            
                Debug.Log("Loading Capsule second time (should be from cache)...");
                stopwatch.Restart();
                var capsule2 = await _addressablesService.LoadAssetAsync<GameObject>("Capsule");
                stopwatch.Stop();
                Debug.Log($"Second load took: {stopwatch.ElapsedMilliseconds}ms (from cache)");
            
                if (capsule1 != null)
                {
                    var capsuleInstance = Instantiate(capsule1, new Vector3(3, 0, 0), Quaternion.identity);
                    capsuleInstance.name = "Loaded Capsule";
                }
            
                // Example 4: Batch preloading
                Debug.Log("=== Example 4: Batch Preloading ===");
                var preloadKeys = new[] { "Cube", "Sphere", "Capsule" };
                var progress = Progress.Create<float>(value => 
                    Debug.Log($"Preload progress: {value:P}"));
                
                await _addressablesService.PreloadAssetsAsync(preloadKeys, progress);
                Debug.Log("Batch preloading complete!");
            
                // Example 5: Show metrics
                ShowMetrics();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Demo error: {ex.Message}");
            }
        }
    
        private void ShowMetrics()
        {
            if (_addressablesService.Analytics != null)
            {
                var metrics = _addressablesService.Analytics.GetMetrics();
                Debug.Log("=== Performance Metrics ===");
                Debug.Log($"Cache Hit Rate: {metrics.CacheHitRate:P}");
                Debug.Log($"Average Load Time: {metrics.AverageLoadTime:F2}ms");
                Debug.Log($"Total Successful Loads: {metrics.TotalLoads}");
                Debug.Log($"Failed Loads: {metrics.FailedLoads}");
            
                if (metrics.LoadTimesByKey != null && metrics.LoadTimesByKey.Count > 0)
                {
                    Debug.Log("Load times by asset:");
                    foreach (var kvp in metrics.LoadTimesByKey)
                    {
                        Debug.Log($"  {kvp.Key}: {kvp.Value:F2}ms");
                    }
                }
            }
        }
    
        private void OnDestroy()
        {
            _addressablesService?.Dispose();
        }
    }
}