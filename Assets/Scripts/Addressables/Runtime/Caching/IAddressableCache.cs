namespace Addressables.Caching
{
    public interface IAddressableCache
    {
        bool TryGet<T>(string key, out T asset) where T : class;
        void Add<T>(string key, T asset) where T : class;
        void Remove(string key);
        void Clear();
        string GetKey(object asset);
        CacheStatistics GetStatistics();
    }
}