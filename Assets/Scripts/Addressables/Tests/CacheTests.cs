using NUnit.Framework;
using UnityEngine;
using Addressables.Caching;
using System.Collections.Generic;

namespace Addressables.Tests
{
    public class CacheTests
    {
        private LRUAddressableCache _cache;
        
        [SetUp]
        public void Setup()
        {
            _cache = new LRUAddressableCache(3); // Small cache for testing
        }
        
        [TearDown]
        public void TearDown()
        {
            _cache.Clear();
        }
        
        [Test]
        public void Add_And_Retrieve_Single_Asset()
        {
            // Arrange
            var testAsset = new GameObject("TestAsset");
            
            // Act
            _cache.Add("key1", testAsset);
            var retrieved = _cache.TryGet<GameObject>("key1", out var asset);
            
            // Assert
            Assert.IsTrue(retrieved);
            Assert.AreSame(testAsset, asset);
            
            // Cleanup
            Object.DestroyImmediate(testAsset);
        }
        
        [Test]
        public void Cache_Should_Return_False_For_Missing_Key()
        {
            // Act
            var result = _cache.TryGet<GameObject>("nonexistent", out var asset);
            
            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(asset);
        }
        
        [Test]
        public void LRU_Should_Evict_Least_Recently_Used()
        {
            // Arrange
            var asset1 = new GameObject("Asset1");
            var asset2 = new GameObject("Asset2");
            var asset3 = new GameObject("Asset3");
            var asset4 = new GameObject("Asset4");
            
            // Act
            _cache.Add("key1", asset1);
            _cache.Add("key2", asset2);
            _cache.Add("key3", asset3);
            
            // Access key1 to make it recently used
            _cache.TryGet<GameObject>("key1", out _);
            
            // Add key4, should evict key2 (least recently used)
            _cache.Add("key4", asset4);
            
            // Assert
            Assert.IsTrue(_cache.TryGet<GameObject>("key1", out _), "key1 should exist");
            Assert.IsFalse(_cache.TryGet<GameObject>("key2", out _), "key2 should be evicted");
            Assert.IsTrue(_cache.TryGet<GameObject>("key3", out _), "key3 should exist");
            Assert.IsTrue(_cache.TryGet<GameObject>("key4", out _), "key4 should exist");
            
            // Cleanup
            Object.DestroyImmediate(asset1);
            Object.DestroyImmediate(asset2);
            Object.DestroyImmediate(asset3);
            Object.DestroyImmediate(asset4);
        }
        
        [Test]
        public void Cache_Should_Update_Existing_Key()
        {
            // Arrange
            var asset1 = new GameObject("Asset1");
            var asset2 = new GameObject("Asset2");
            
            // Act
            _cache.Add("key1", asset1);
            _cache.Add("key1", asset2); // Update with new asset
            
            // Assert
            _cache.TryGet<GameObject>("key1", out var retrieved);
            Assert.AreSame(asset2, retrieved);
            
            // Cleanup
            Object.DestroyImmediate(asset1);
            Object.DestroyImmediate(asset2);
        }
        
        [Test]
        public void GetStatistics_Should_Return_Correct_Values()
        {
            // Arrange
            var asset1 = new GameObject("Asset1");
            var asset2 = new GameObject("Asset2");
            
            // Act
            _cache.Add("key1", asset1);
            _cache.Add("key2", asset2);
            _cache.TryGet<GameObject>("key1", out _); // Hit
            _cache.TryGet<GameObject>("key1", out _); // Hit
            _cache.TryGet<GameObject>("key3", out _); // Miss
            
            var stats = _cache.GetStatistics();
            
            // Assert
            Assert.AreEqual(2, stats.TotalEntries);
            Assert.AreEqual(3, stats.MaxSize);
            Assert.Greater(stats.HitRate, 0);
            
            // Cleanup
            Object.DestroyImmediate(asset1);
            Object.DestroyImmediate(asset2);
        }
        
        [Test]
        public void Cache_Should_Handle_Null_Assets_Gracefully()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _cache.Add("key1", null as GameObject));
            
            var result = _cache.TryGet<GameObject>("key1", out var asset);
            Assert.IsFalse(result);
        }
    }
}