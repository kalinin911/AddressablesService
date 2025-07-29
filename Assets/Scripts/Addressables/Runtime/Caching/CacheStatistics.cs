using System.Collections.Generic;

namespace Addressables.Caching
{
    public class CacheStatistics
    {
        public int TotalEntries { get; set; }
        public int MaxSize { get; set; }
        public float HitRate { get; set; }
        public List<string> MostAccessedKeys { get; set; }
        
        public CacheStatistics()
        {
            MostAccessedKeys = new List<string>();
        }
    }
}