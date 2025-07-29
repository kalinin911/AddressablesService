using System.Collections.Generic;

namespace Addressables.Analytics
{
    public class AddressableMetricsData
    {
        public float CacheHitRate { get; set; }
        public float AverageLoadTime { get; set; }
        public int TotalLoads { get; set; }
        public int FailedLoads { get; set; }
        public Dictionary<string, float> LoadTimesByKey { get; set; }
        
        public AddressableMetricsData()
        {
            LoadTimesByKey = new Dictionary<string, float>();
        }
    }
}