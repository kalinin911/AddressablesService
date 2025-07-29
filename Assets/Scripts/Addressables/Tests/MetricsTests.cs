using NUnit.Framework;
using Addressables.Analytics;
using System;
using System.Threading;

namespace Addressables.Tests
{
    public class MetricsTests
    {
        private AddressableMetrics _metrics;
        
        [SetUp]
        public void Setup()
        {
            _metrics = new AddressableMetrics();
        }
        
        [TearDown]
        public void TearDown()
        {
            _metrics.Dispose();
        }
        
        [Test]
        public void Should_Calculate_Cache_Hit_Rate_Correctly()
        {
            // Act
            _metrics.RecordCacheHit("asset1");
            _metrics.RecordCacheHit("asset2");
            _metrics.RecordCacheMiss("asset3");
            
            // Assert
            var data = _metrics.GetMetrics();
            Assert.AreEqual(0.667f, data.CacheHitRate, 0.01f); // 2 hits / 3 total
        }
        
        [Test]
        public void Should_Track_Load_Times_Correctly()
        {
            // Act
            _metrics.RecordLoadTime("asset1", 100f);
            _metrics.RecordLoadTime("asset1", 200f);
            _metrics.RecordLoadTime("asset2", 50f);
            
            // Assert
            var data = _metrics.GetMetrics();
            Assert.AreEqual(116.67f, data.AverageLoadTime, 0.01f); // (100+200+50)/3
            Assert.AreEqual(150f, data.LoadTimesByKey["asset1"], 0.01f); // (100+200)/2
            Assert.AreEqual(50f, data.LoadTimesByKey["asset2"], 0.01f);
        }
        
        [Test]
        public void Should_Track_Failed_Loads()
        {
            // Arrange
            UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Error, "Failed to load asset3: Test error");

            // Act
            _metrics.RecordSuccessfulLoad("asset1");
            _metrics.RecordSuccessfulLoad("asset2");
            _metrics.RecordFailedLoad("asset3", new Exception("Test error"));
    
            // Assert
            var data = _metrics.GetMetrics();
            Assert.AreEqual(2, data.TotalLoads); // 2 successful loads
            Assert.AreEqual(1, data.FailedLoads); // 1 failed load
        }
        
        [Test]
        public void MeasureLoadTime_Should_Record_Elapsed_Time()
        {
            // Act
            using (_metrics.MeasureLoadTime("asset1"))
            {
                Thread.Sleep(50); // Simulate work
            }
            
            // Assert
            var data = _metrics.GetMetrics();
            Assert.IsTrue(data.LoadTimesByKey.ContainsKey("asset1"));
            Assert.GreaterOrEqual(data.LoadTimesByKey["asset1"], 50f);
        }
        
        [Test]
        public void Should_Handle_Empty_Metrics_Gracefully()
        {
            // Act
            var data = _metrics.GetMetrics();
            
            // Assert
            Assert.AreEqual(0f, data.CacheHitRate);
            Assert.AreEqual(0f, data.AverageLoadTime);
            Assert.AreEqual(0, data.TotalLoads);
            Assert.AreEqual(0, data.FailedLoads);
            Assert.IsNotNull(data.LoadTimesByKey);
            Assert.AreEqual(0, data.LoadTimesByKey.Count);
        }
    }
}