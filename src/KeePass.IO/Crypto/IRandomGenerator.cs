using System;
using Windows.Storage.Streams;

namespace KeePass.IO.Crypto
{
    public interface IRandomGenerator
    {
        /// <summary>
        /// Get a buffer of random bytes of the specified size.
        /// </summary>
        /// <param name="size">Size of the bytes buffer.</param>
        /// <returns>Random bytes buffer.</returns>
        IBuffer GetRandomBytes(int size);
    }
}