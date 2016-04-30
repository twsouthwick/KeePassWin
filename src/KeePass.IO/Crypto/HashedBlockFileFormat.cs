using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace KeePass.Crypto
{
    public class HashedBlockFileFormat
    {
        /// <summary>
        /// Reads the specified hashed block stream into a memory stream.
        /// </summary>
        /// <param name="input">The hashed block stream.</param>
        /// <returns>The de-hashed stream.</returns>
        public static async Task<Stream> Read(Stream inputStream)
        {
            if (inputStream == null)
                throw new ArgumentNullException("input");

            var input = inputStream.AsInputStream();

            var blockIndex = 0;
            var result = new MemoryStream();
            var hash = WindowsRuntimeBuffer.Create(32);

            var reader = new DataReader(input)
            {
                ByteOrder = ByteOrder.LittleEndian,
            };
            var sha = HashAlgorithmProvider
                .OpenAlgorithm(HashAlgorithmNames.Sha256);

            try
            {
                while (true)
                {
                    // Detect end of file
                    var read = await reader.LoadAsync(4);
                    if (read == 0)
                        break;

                    // Verify block index
                    var index = reader.ReadInt32();
                    if (index != blockIndex)
                    {
                        throw new InvalidDataException(string.Format(
                            "Wrong block ID detected, expected: {0}, actual: {1}",
                            blockIndex, index));
                    }
                    blockIndex++;

                    // Block hash
                    hash = await input.ReadAsync(hash, 32);
                    if (hash.Length != 32)
                    {
                        throw new InvalidDataException(
                            "Data corruption detected (truncated data)");
                    }

                    read = await reader.LoadAsync(4);
                    if (read != 4)
                    {
                        throw new InvalidDataException(
                            "Data corruption detected (truncated data)");
                    }

                    // Validate block size (< 10MB)
                    var blockSize = reader.ReadInt32();
                    if (blockSize == 0)
                    {
                        // Terminator block
                        var isTerminator = hash
                            .ToArray()
                            .All(x => x == 0);

                        if (!isTerminator)
                        {
                            throw new InvalidDataException(
                                "Data corruption detected (invalid hash for terminator block)");
                        }

                        break;
                    }

                    if (0 > blockSize || blockSize > 10485760)
                    {
                        throw new InvalidDataException(
                            "Data corruption detected (truncated data)");
                    }

                    // Check data truncate
                    var loaded = await reader.LoadAsync((uint)blockSize);
                    if (loaded < blockSize)
                    {
                        throw new InvalidDataException(
                            "Data corruption detected (truncated data)");
                    }

                    var buffer = reader.ReadBuffer((uint)blockSize);

                    // Verify block integrity
                    var actual = sha.HashData(buffer);
                    if (!CryptographicBuffer.Compare(hash, actual))
                    {
                        throw new InvalidDataException(
                            "Data corruption detected (content corrupted)");
                    }

                    await result.WriteAsync(buffer.ToArray(),
                        0, (int)buffer.Length);
                }

                result.Position = 0;
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }
    }
}