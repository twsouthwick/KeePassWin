using System;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace KeePass
{
    public static class Utils
    {
        /// <summary>
        /// Returns an asynchronous byte reader object. 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer">The buffer into which the asynchronous read operation places the bytes that are read.</param>
        /// <param name="count">The number of bytes to read that is less than or equal to the <see cref="IBuffer.Capacity"/> value.</param>
        /// <returns>The asynchronous operation</returns>
        public static IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(
            this IInputStream stream, IBuffer buffer, uint count)
        {
            return stream.ReadAsync(buffer, count, InputStreamOptions.None);
        }

        /// <summary>
        /// Returns an asynchronous byte reader object. 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer">The buffer into which the asynchronous read operation places the bytes that are read.</param>
        /// <returns>The asynchronous operation</returns>
        public static IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(
            this IInputStream stream, IBuffer buffer)
        {
            return ReadAsync(stream, buffer, buffer.Capacity);
        }
    }
}