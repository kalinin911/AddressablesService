using System;
using System.Threading;
using Addressables.Analytics;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Addressables.Core
{
    public interface IAddressablesService : IDisposable
    {
        event Action<string, float> OnLoadProgress;
        event Action<string, object> OnAssetLoaded;
        event Action<string, Exception> OnLoadFailed;
        
        UniTask<T> LoadAssetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
        UniTask<T> LoadAssetAsync<T>(AssetReference reference, CancellationToken cancellationToken = default) where T : class;
        UniTask<IAddressableHandle<T>> LoadWithHandleAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
        
        void ReleaseAsset<T>(T asset) where T : class;
        void ReleaseHandle<T>(IAddressableHandle<T> handle) where T : class;
        
        UniTask PreloadAssetsAsync(string[] keys, IProgress<float> progress = null, CancellationToken cancellationToken = default);
        UniTask WarmupAsync(CancellationToken cancellationToken = default);
        
        bool TryGetCached<T>(string key, out T asset) where T : class;
        void ClearCache();
        
        IAddressableAnalytics Analytics { get; }
    }
}