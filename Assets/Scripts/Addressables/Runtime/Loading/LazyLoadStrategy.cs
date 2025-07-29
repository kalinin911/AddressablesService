using System;
using System.Threading;
using Addressables.Core;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Addressables.Loading
{
    public class LazyLoadStrategy : ILoadingStrategy
    {
        public async UniTask<AsyncOperationHandle<T>> LoadAsync<T>(
            string key, 
            Action<float> onProgress = null,
            CancellationToken cancellationToken = default) where T : class
        {
            var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(key);
            
            while (!handle.IsDone && !cancellationToken.IsCancellationRequested)
            {
                onProgress?.Invoke(handle.PercentComplete);
                await UniTask.Yield(cancellationToken);
            }
            
            cancellationToken.ThrowIfCancellationRequested();
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return handle;
            }
            
            throw new AddressableLoadException($"Failed to load asset: {key}");
        }
    }
}