using System;

namespace Addressables.Core
{
    public class AddressableHandle<T> : IAddressableHandle<T> where T : class
    {
        private readonly IAddressablesService _service;
        private int _refCount = 1;
        private bool _disposed;
        
        public T Asset { get; private set; }
        public string Key { get; private set; }
        public bool IsValid => !_disposed && Asset != null;
        
        public AddressableHandle(T asset, string key, IAddressablesService service)
        {
            Asset = asset ?? throw new ArgumentNullException(nameof(asset));
            Key = key ?? throw new ArgumentNullException(nameof(key));
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }
        
        public void AddRef()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AddressableHandle<T>));
                
            _refCount++;
        }
        
        public void Release()
        {
            if (_disposed) return;
            
            _refCount--;
            if (_refCount <= 0)
            {
                Dispose();
            }
        }
        
        public void Dispose()
        {
            if (_disposed) return;
            
            _disposed = true;
            _service.ReleaseHandle(this);
            Asset = null;
        }
    }
}