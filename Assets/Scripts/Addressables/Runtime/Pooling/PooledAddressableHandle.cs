using Addressables.Core;

namespace Addressables.Pooling
{
    public class PooledAddressableHandle<T> : AddressableHandle<T> where T : class
    {
        public PooledAddressableHandle(T asset, string key, IAddressablesService service) 
            : base(asset, key, service)
        {
        }
        
        public void Reset()
        {
            // Reset state for reuse
        }
        
        public void Initialize(T asset, string key, IAddressablesService service)
        {
            // Re-initialize for reuse from pool
        }
    }
}