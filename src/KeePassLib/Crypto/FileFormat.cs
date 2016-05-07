using KeePass.Crypto;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KeePass
{
    public class FileFormat
    {
        private readonly ICryptoProvider _hashProvider;
        private readonly Func<Stream, HashedStream> _inputStreamFactory;

        public FileFormat(Func<Stream, HashedStream> inputStreamFactory, ICryptoProvider hashProvider)
        {
            _inputStreamFactory = inputStreamFactory;
            _hashProvider = hashProvider;
        }

        /// <summary>
        /// Decrypts the specified input stream.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="masterKey">The master key.</param>
        /// <param name="headers">The database file headers.</param>
        /// <returns>The decrypted buffer.</returns>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="input"/>, <paramref name="masterKey"/>,
        /// <paramref name="headers"/> cannot be <c>null</c>.
        /// </exception>
        public Task<byte[]> Decrypt(Stream input, byte[] masterKey, FileHeaders headers)
        {
            if (headers == null)
                throw new ArgumentNullException("headers");

            return Decrypt(input, masterKey, headers.MasterSeed, headers.EncryptionIV);
        }

        /// <summary>
        /// Decrypts the specified input stream.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="masterKey">The master key.</param>
        /// <param name="masterSeed">The master seed.</param>
        /// <param name="encryptionIV">The encryption initialization vector.</param>
        /// <returns>The decrypted buffer.</returns>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="input"/>, <paramref name="masterSeed"/>, <paramref name="masterKey"/>
        /// and <paramref name="encryptionIV"/> cannot be <c>null</c>.
        /// </exception>
        public Task<byte[]> Decrypt(Stream input, byte[] masterKey, byte[] masterSeed, byte[] encryptionIV)
        {
            if (input == null) throw new ArgumentNullException("input");
            if (masterSeed == null) throw new ArgumentNullException("masterSeed");
            if (masterKey == null) throw new ArgumentNullException("masterKey");
            if (encryptionIV == null) throw new ArgumentNullException("encryptionIV");

            var sha = _hashProvider.GetSha256();

            sha.Append(masterSeed);
            sha.Append(masterKey);

            var seed = sha.GetValueAndReset();

            return Task.FromResult(_hashProvider.Decrypt(input, seed, encryptionIV));
        }

        /// <summary>
        /// Reads the headers of the specified database file stream.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <returns>The read result.</returns>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="input"/> cannot be <c>null</c>.
        /// </exception>
        public async Task<ReadHeaderResult> Headers(Stream input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var hash = _inputStreamFactory(input);

            // Signature
            var signature = await hash.ReadAsync(8);

            var format = CheckSignature(signature);
            if (format != FileFormats.Supported)
            {
                return new ReadHeaderResult
                {
                    Format = format,
                };
            }

            // Schema version
            var versionBytes = await hash.ReadAsync(4);
            var version = GetVersion(versionBytes);
            format = CheckCompatibility(version);
            switch (format)
            {
                case FileFormats.Supported:
                case FileFormats.PartialSupported:
                    break;

                default:
                    return new ReadHeaderResult
                    {
                        Format = format,
                    };
            }

            // Fields
            var headers = await GetHeaders(hash);
            headers.Hash = hash.GetHashAndReset();

            return new ReadHeaderResult
            {
                Format = format,
                Headers = headers,
            };
        }

        /// <summary>
        /// Parses the decrypted content.
        /// </summary>
        /// <param name="decrypted">The input stream.</param>
        /// <param name="useGZip">Set to <c>true</c> to decompress the input stream before parsing.</param>
        /// <returns>The decrypted content.</returns>
        /// <param name="headers">The database file headers.</param>
        /// <exception cref="System.ArgumentNullException">
        /// The <paramref name="decrypted"/> or <paramref name="headers"/> parameter cannot be <c>null</c>.
        /// </exception>
        public async Task<XDocument> ParseContent(
            Stream decrypted, bool useGZip, FileHeaders headers)
        {
            if (decrypted == null) throw new ArgumentNullException("decrypted");
            if (headers == null) throw new ArgumentNullException("headers");

            var deHashed = await ReadHashedBlockFileFormat(decrypted);
            var input = deHashed;

            try
            {
                if (useGZip)
                {
                    input = new GZipStream(input,
                        CompressionMode.Decompress);
                }

                var doc = XDocument.Load(input);
                Decrypt(headers, doc);

                return doc;
            }
            finally
            {
                deHashed.Dispose();
                input.Dispose();
            }
        }

        /// <summary>
        /// Verifies the database file headers integrity.
        /// </summary>
        /// <param name="headers">The database file headers.</param>
        /// <param name="doc">The database content.</param>
        /// <returns><c>true</c> if the header is valid; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="headers"/> and <paramref name="doc"/> cannot be <c>null</c>.
        /// </exception>
        public bool VerifyHeaders(FileHeaders headers, XDocument doc)
        {
            return VerifyHeaders(headers.Hash, doc);
        }

        /// <summary>
        /// Verifies the database file headers integrity.
        /// </summary>
        /// <param name="headerHash">The database file headers hash.</param>
        /// <param name="doc">The database content.</param>
        /// <returns><c>true</c> if the header is valid; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="headerHash"/> and <paramref name="doc"/> cannot be <c>null</c>.
        /// </exception>
        public bool VerifyHeaders(byte[] headerHash, XDocument doc)
        {
            string meta;
            try
            {
                meta = doc
                    .Elements("KeePassFile")
                    .Elements("Meta")
                    .Elements("HeaderHash")
                    .Select(x => x.Value)
                    .Single();
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            var expected = Convert.FromBase64String(meta);

            return ByteArrayEquals(expected, headerHash);
        }

        /// <summary>
        /// Verifies the start bytes of the decrypted content stream.
        /// </summary>
        /// <param name="input">The decrypted content stream.</param>
        /// <param name="startBytes">The start bytes stored in database file headers.</param>
        /// <returns><c>true</c> if the bytes match; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="input"/> and <paramref name="startBytes"/> cannot be <c>null</c>.
        /// </exception>
        public async Task<bool> VerifyStartBytes(Stream input, byte[] startBytes)
        {
            if (input == null) throw new ArgumentNullException("input");
            if (startBytes == null) throw new ArgumentNullException("startBytes");

            var actual = await input.ReadAsync(startBytes.Length);

            if (actual.Length != startBytes.Length)
            {
                return false;
            }

            return ByteArrayEquals(startBytes, actual);
        }

        /// <summary>
        /// Verifies the start bytes of the decrypted content stream.
        /// </summary>
        /// <param name="input">The decrypted content stream.</param>
        /// <param name="headers">The database file headers.</param>
        /// <returns><c>true</c> if the bytes match; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="input"/> and <paramref name="headers"/> cannot be <c>null</c>.
        /// </exception>
        public Task<bool> VerifyStartBytes(Stream input, FileHeaders headers)
        {
            if (headers == null)
                throw new ArgumentNullException("headers");

            return VerifyStartBytes(input, headers.StartBytes);
        }

        /// <summary>
        /// Gets the compatibility with the specified database schema version.
        /// </summary>
        /// <param name="schema">The database schema version.</param>
        /// <returns>The compatibility.</returns>
        private FileFormats CheckCompatibility(Version schema)
        {
            if (schema.Major < 3)
                return FileFormats.OldVersion;

            if (schema.Major > 3)
                return FileFormats.NewVersion;

            return schema.Minor > 1
                ? FileFormats.PartialSupported
                : FileFormats.Supported;
        }

        static bool ByteArrayEquals(byte[] a1, byte[] a2)
        {
            return System.Collections.StructuralComparisons.StructuralEqualityComparer.Equals(a1, a2);
        }

        private static class KeepassHeaders
        {
            // KeePass 1.x
            public static readonly byte[] Version1 = new byte[] { 0x03, 0xD9, 0xA2, 0x9A, 0x65, 0xFB, 0x4B, 0xB5 };

            // KeePass 2.x pre-release
            public static readonly byte[] Version2pre = new byte[] { 0x03, 0xD9, 0xA2, 0x9A, 0x66, 0xFB, 0x4B, 0xB5 };

            // KeePass 2.x
            public static readonly byte[] Version2 = new byte[] { 0x03, 0xD9, 0xA2, 0x9A, 0x67, 0xFB, 0x4B, 0xB5 };
        }

        /// <summary>
        /// Gets the file format of the specified stream based on file signature.
        /// </summary>
        /// <param name="buffer">The signature bytes buffer.</param>
        /// <returns>The detected database file format.</returns>
        private FileFormats CheckSignature(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (ByteArrayEquals(KeepassHeaders.Version1, data))
                return FileFormats.KeePass1x;

            if (ByteArrayEquals(KeepassHeaders.Version2pre, data))
                return FileFormats.OldVersion;

            if (!ByteArrayEquals(KeepassHeaders.Version2, data))
                return FileFormats.NotSupported;

            return FileFormats.Supported;
        }

        private void Decrypt(FileHeaders headers, XDocument doc)
        {
            var protectedStrings = doc.Descendants("Entry")
                .SelectMany(x => x.Elements("String"))
                .Select(x => x.Element("Value"))
                .Where(x =>
                {
                    var protect = x.Attribute("Protected");
                    return protect != null && (bool)protect;
                });

            var generator = GetRandomNumberGenerator(headers.RandomAlgorithm, headers.ProtectedStreamKey);

            foreach (var protectedString in protectedStrings)
            {
                var encrypted = Convert.FromBase64String(
                    protectedString.Value);
                var length = encrypted.Length;

                var padding = generator.GetRandomBytes(length);

                for (var i = 0U; i < length; i++)
                    encrypted[i] ^= padding[i];

                protectedString.Value = Encoding.UTF8
                    .GetString(encrypted, 0, length);
            }
        }

        private IRandomGenerator GetRandomNumberGenerator(CrsAlgorithm algorithm, byte[] protectedStreamKey)
        {
            switch (algorithm)
            {
                case CrsAlgorithm.ArcFourVariant:
                    return new Rc4RandomGenerator(protectedStreamKey);
                default:
                    return new Salsa20RandomGenerator(_hashProvider, protectedStreamKey);
            }
        }

        /// <summary>
        /// Parse the headers fields.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="buffer">The header bytes reader.</param>
        /// <returns>The file headers.</returns>
        private async Task<FileHeaders> GetHeaders(Stream input)
        {
            var result = new FileHeaders();

            while (true)
            {
                var buffer = await input.ReadAsync(3);
                var field = (HeaderFields)buffer[0];
                var size = BitConverter.ToUInt16(new[] { buffer[1], buffer[2] }, 0);

                if (size > 0)
                {
                    buffer = await input.ReadAsync(size);
                }

                switch (field)
                {
                    case HeaderFields.EndOfHeader:
                        return result;

                    case HeaderFields.CompressionFlags:
                        result.UseGZip = buffer[0] == 1;
                        break;

                    case HeaderFields.EncryptionIV:
                        result.EncryptionIV = buffer;
                        break;

                    case HeaderFields.MasterSeed:
                        result.MasterSeed = buffer;
                        break;

                    case HeaderFields.StreamStartBytes:
                        result.StartBytes = buffer;
                        break;

                    case HeaderFields.TransformSeed:
                        result.TransformSeed = buffer;
                        break;

                    case HeaderFields.TransformRounds:
                        result.TransformRounds = BitConverter.ToUInt64(buffer, 0);
                        break;

                    case HeaderFields.ProtectedStreamKey:
                        result.ProtectedStreamKey = buffer;
                        break;

                    case HeaderFields.InnerRandomStreamID:
                        result.RandomAlgorithm = (CrsAlgorithm)BitConverter.ToUInt32(buffer, 0);
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the database schema version.
        /// </summary>
        /// <param name="bytes">The version bytes</param>
        /// <returns>The database schema version.</returns>
        private Version GetVersion(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            var minor = BitConverter.ToUInt16(bytes, 0);
            var major = BitConverter.ToUInt16(bytes, 2);

            return new Version(major, minor);
        }

        /// <summary>
        /// Reads the specified hashed block stream into a memory stream.
        /// </summary>
        /// <param name="input">The hashed block stream.</param>
        /// <returns>The de-hashed stream.</returns>
        public async Task<Stream> ReadHashedBlockFileFormat(Stream input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var blockIndex = 0;
            var result = new MemoryStream();
            //var hash = WindowsRuntimeBuffer.Create(32);

            using (var reader = new BinaryReader(input))
            {
                var sha = _hashProvider.GetSha256();

                while (true)
                {
                    // Verify block index
                    var index = reader.ReadInt32();

                    if (index != blockIndex)
                    {
                        throw new InvalidDataException($"Wrong block ID detected, expected: {blockIndex}, actual: {index}");
                    }

                    blockIndex++;

                    // Block hash
                    var hash = reader.ReadBytes(32);
                    if (hash.Length != 32)
                    {
                        throw new InvalidDataException("Data corruption detected (truncated data)");
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
                            throw new InvalidDataException("Data corruption detected (invalid hash for terminator block)");
                        }

                        break;
                    }

                    if (0 > blockSize || blockSize > 10485760)
                    {
                        throw new InvalidDataException(
                            "Data corruption detected (truncated data)");
                    }

                    // Check data truncate
                    var buffer = reader.ReadBytes(blockSize);
                    if (buffer.Length < blockSize)
                    {
                        throw new InvalidDataException("Data corruption detected (truncated data)");
                    }

                    // Verify block integrity
                    var actual = _hashProvider.GetSha256(buffer);
                    if (!ByteArrayEquals(hash, actual))
                    {
                        throw new InvalidDataException("Data corruption detected (content corrupted)");
                    }

                    await result.WriteAsync(buffer.ToArray(), 0, (int)buffer.Length);
                }

                result.Position = 0;
                return result;
            }
        }
    }
}