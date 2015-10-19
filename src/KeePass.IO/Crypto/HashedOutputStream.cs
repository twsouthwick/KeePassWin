using System;
using Windows.Foundation;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace KeePass.IO.Crypto
{
    /// <summary>
    /// <see cref="IOutputStream"/> wrapper that provides the SH 256 hash
    /// of the data written to the stream. 
    /// </summary>
    public class HashedOutputStream : IOutputStream
    {
        private readonly CryptographicHash _sha;
        private readonly IOutputStream _stream;

        public HashedOutputStream(IOutputStream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            _stream = stream;
            _sha = HashAlgorithmProvider
                .OpenAlgorithm(HashAlgorithmNames.Sha256)
                .CreateHash();
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        /// <summary>
        /// Flushes data asynchronously in a sequential stream.
        /// </summary>
        /// <returns>
        /// The stream flush operation.
        /// </returns>
        public IAsyncOperation<bool> FlushAsync()
        {
            return _stream.FlushAsync();
        }

        /// <summary>
        /// Gets hash of written bytes and reset.
        /// </summary>
        /// <returns>Hash of the written bytes.</returns>
        public IBuffer GetHashAndReset()
        {
            return _sha.GetValueAndReset();
        }

        /// <summary>
        /// Writes data asynchronously in a sequential stream.
        /// </summary>
        /// <param name="buffer">The buffer into which the asynchronous writer operation writes.</param>
        /// <returns>
        /// The byte writer operation.
        /// </returns>
        public IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
        {
            _sha.Append(buffer);
            return _stream.WriteAsync(buffer);
        }
    }
}