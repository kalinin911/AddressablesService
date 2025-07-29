using System;

namespace Addressables.Pooling
{
    [Serializable]
    public class PoolConfiguration
    {
        public int InitialSize = 10;
        public int MaxSize = 50;
        public bool WarmupOnStart = true;
        public float CleanupInterval = 60f;
    }
}