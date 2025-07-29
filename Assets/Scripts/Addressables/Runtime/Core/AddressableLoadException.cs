using System;

namespace Addressables.Core
{
    public class AddressableLoadException : Exception
    {
        public AddressableLoadException(string message) : base(message) { }
        public AddressableLoadException(string message, Exception innerException) : base(message, innerException) { }
    }
}