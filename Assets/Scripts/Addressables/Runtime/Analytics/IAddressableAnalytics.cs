using System;

namespace Addressables.Analytics
{
    public interface IAddressableAnalytics
    {
        void RecordLoadTime(string key, float timeMs);
        void RecordCacheHit(string key);
        void RecordCacheMiss(string key);
        void RecordSuccessfulLoad(string key);
        void RecordFailedLoad(string key, Exception exception);
        AddressableMetricsData GetMetrics();
        IDisposable MeasureLoadTime(string key);
    }
}