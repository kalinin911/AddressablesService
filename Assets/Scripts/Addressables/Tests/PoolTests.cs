using NUnit.Framework;
using UnityEngine;
using Addressables.Pooling;
using Addressables.Core;
using NSubstitute;

namespace Addressables.Tests
{
    public class PoolTests
    {
        private AddressablePool _pool;
        private PoolConfiguration _config;
        private IAddressablesService _mockService;
        
        [SetUp]
        public void Setup()
        {
            _config = new PoolConfiguration
            {
                InitialSize = 5,
                MaxSize = 10,
                WarmupOnStart = false
            };
            _pool = new AddressablePool(_config);
            _mockService = Substitute.For<IAddressablesService>();
        }
        
        [TearDown]
        public void TearDown()
        {
            _pool.Dispose();
        }
        
        [Test]
        public void Should_Return_False_When_Pool_Is_Empty()
        {
            // Act
            var result = _pool.TryGet<GameObject>(out var handle);
            
            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(handle);
        }
        
        [Test]
        public void Should_Reuse_Returned_Handles()
        {
            // Arrange
            var asset = new GameObject("TestAsset");
            var handle = new PooledAddressableHandle<GameObject>(asset, "key1", _mockService);
            
            // Act
            _pool.Return(handle);
            var retrieved = _pool.TryGet<GameObject>(out var retrievedHandle);
            
            // Assert
            Assert.IsTrue(retrieved);
            Assert.AreSame(handle, retrievedHandle);
            
            // Cleanup
            Object.DestroyImmediate(asset);
        }
        
        [Test]
        public void Should_Not_Exceed_Max_Pool_Size()
        {
            // Arrange
            var handles = new PooledAddressableHandle<GameObject>[15];
            for (int i = 0; i < 15; i++)
            {
                var asset = new GameObject($"Asset{i}");
                handles[i] = new PooledAddressableHandle<GameObject>(asset, $"key{i}", _mockService);
            }
            
            // Act - Return more handles than max size
            foreach (var handle in handles)
            {
                _pool.Return(handle);
            }
            
            // Assert - Should only keep MaxSize handles
            int retrievedCount = 0;
            while (_pool.TryGet<GameObject>(out _))
            {
                retrievedCount++;
            }
            
            Assert.AreEqual(_config.MaxSize, retrievedCount);
            
            // Cleanup
            for (int i = 0; i < 15; i++)
            {
                Object.DestroyImmediate(handles[i].Asset);
            }
        }
        
        [Test]
        public void Clear_Should_Empty_Pool()
        {
            // Arrange
            var asset = new GameObject("TestAsset");
            var handle = new PooledAddressableHandle<GameObject>(asset, "key1", _mockService);
            _pool.Return(handle);
            
            // Act
            _pool.Clear();
            
            // Assert
            var result = _pool.TryGet<GameObject>(out _);
            Assert.IsFalse(result);
            
            // Cleanup
            Object.DestroyImmediate(asset);
        }
    }
}