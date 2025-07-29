using System;

namespace Addressables.Core
{
    public interface IAddressableHandle<out T> : IDisposable where T : class
    {
        T Asset { get; }
        string Key { get; }
        bool IsValid { get; }
        void AddRef();
        void Release();
    }
}