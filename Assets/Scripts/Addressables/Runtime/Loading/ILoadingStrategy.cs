using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Addressables.Loading
{
    public interface ILoadingStrategy
    {
        UniTask<AsyncOperationHandle<T>> LoadAsync<T>(
            string key, 
            Action<float> onProgress = null,
            CancellationToken cancellationToken = default) where T : class;
    }
}