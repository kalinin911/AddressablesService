using NUnit.Framework;
using UnityEngine;
using Addressables.Core;
using NSubstitute;

namespace Addressables.Tests
{
    public class HandleTests
    {
        private IAddressablesService _mockService;
        
        [SetUp]
        public void Setup()
        {
            _mockService = Substitute.For<IAddressablesService>();
        }
        
        [Test]
        public void Handle_Should_Start_With_RefCount_One()
        {
            // Arrange
            var asset = new GameObject("TestAsset");
            var handle = new AddressableHandle<GameObject>(asset, "key1", _mockService);
            
            // Assert
            Assert.IsTrue(handle.IsValid);
            Assert.AreSame(asset, handle.Asset);
            Assert.AreEqual("key1", handle.Key);
            
            // Cleanup
            Object.DestroyImmediate(asset);
        }
        
        [Test]
        public void Handle_Should_Be_Invalid_After_Dispose()
        {
            // Arrange
            var asset = new GameObject("TestAsset");
            var handle = new AddressableHandle<GameObject>(asset, "key1", _mockService);
            
            // Act
            handle.Dispose();
            
            // Assert
            Assert.IsFalse(handle.IsValid);
            _mockService.Received(1).ReleaseHandle(handle);
            
            // Cleanup
            Object.DestroyImmediate(asset);
        }
        
        [Test]
        public void Handle_Should_Support_Reference_Counting()
        {
            // Arrange
            var asset = new GameObject("TestAsset");
            var handle = new AddressableHandle<GameObject>(asset, "key1", _mockService);
            
            // Act
            handle.AddRef(); // refCount = 2
            handle.Release(); // refCount = 1
            
            // Assert - should still be valid
            Assert.IsTrue(handle.IsValid);
            _mockService.DidNotReceive().ReleaseHandle(handle);
            
            // Act - final release
            handle.Release(); // refCount = 0
            
            // Assert - should be disposed
            Assert.IsFalse(handle.IsValid);
            _mockService.Received(1).ReleaseHandle(handle);
            
            // Cleanup
            Object.DestroyImmediate(asset);
        }
        
        [Test]
        public void Handle_Should_Throw_On_AddRef_After_Dispose()
        {
            // Arrange
            var asset = new GameObject("TestAsset");
            var handle = new AddressableHandle<GameObject>(asset, "key1", _mockService);
            handle.Dispose();
            
            // Act & Assert
            Assert.Throws<System.ObjectDisposedException>(() => handle.AddRef());
            
            // Cleanup
            Object.DestroyImmediate(asset);
        }
    }
}