# Unity Addressables Service

A wrapper that fixes common Addressables pain points - duplicate loads, memory leaks, and crashes from early releases.

## Features

- **Smart caching** - Load once, use everywhere
- **Auto reference counting** - No more "asset already released" errors  
- **Built-in metrics** - Track load times and cache performance
- **Thread-safe** - Handle concurrent loads without issues

Built this after dealing with Addressables crashes.

## Installation

1. Install Unity Addressables package
2. Install [UniTask](https://github.com/Cysharp/UniTask) via Package Manager
3. Clone this repository into your `Assets/Scripts` folder
4. Create configuration: `Assets > Create > Addressables > Configuration`

## Quick Start

```csharp
// Initialize
var config = Resources.Load<AddressablesConfiguration>("AddressablesConfig");
var service = new AddressablesService(config);

// Load asset (with automatic caching)
var prefab = await service.LoadAssetAsync<GameObject>("EnemyPrefab");

// Load with handle (auto-releases when disposed)
using (var handle = await service.LoadWithHandleAsync<Texture2D>("Icon"))
{
   image.texture = handle.Asset;
}

// Check metrics
var metrics = service.Analytics.GetMetrics();
Debug.Log($"Cache hit rate: {metrics.CacheHitRate:P}");