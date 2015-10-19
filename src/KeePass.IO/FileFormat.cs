using System;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using KeePass.IO.Crypto;
using KeePass.IO.Models;

namespace KeePass.IO
{
    public static class FileFormat
    {
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
        public static Task<IInputStream> Decrypt(IRandomAccessStream input,
            IBuffer masterKey, FileHeaders headers)
        {
            if (headers == null)
                throw new ArgumentNullException("headers");

            return Decrypt(input, masterKey,
                headers.MasterSeed, headers.EncryptionIV);
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
        public static async Task<IInputStream> Decrypt(IRandomAccessStream input,
            IBuffer masterKey, IBuffer masterSeed, IBuffer encryptionIV)
        {
            if (input == null) throw new ArgumentNullException("input");
            if (masterSeed == null) throw new ArgumentNullException("masterSeed");
            if (masterKey == null) throw new ArgumentNullException("masterKey");
            if (encryptionIV == null) throw new ArgumentNullException("encryptionIV");

            var sha = HashAlgorithmProvider
                .OpenAlgorithm(HashAlgorithmNames.Sha256)
                .CreateHash();

            sha.Append(masterSeed);
            sha.Append(masterKey);

            var seed = sha.GetValueAndReset();
            var aes = SymmetricKeyAlgorithmProvider
                .OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7)
                .CreateSymmetricKey(seed);

            var buffer = WindowsRuntimeBuffer.Create(
                (int)(input.Size - input.Position));
            buffer = await input.ReadAsync(buffer, buffer.Capacity);
            buffer = CryptographicEngine.Decrypt(aes, buffer, encryptionIV);

            var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(buffer);
            stream.Seek(0);

            return stream;
        }

        /// <summary>
        /// Reads the headers of the specified database file stream.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <returns>The read result.</returns>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="input"/> cannot be <c>null</c>.
        /// </exception>
        public static async Task<ReadHeaderResult> Headers(IInputStream input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            var hash = new HashedInputStream(input);
            var buffer = WindowsRuntimeBuffer.Create(128);

            // Signature
            buffer = await hash.ReadAsync(buffer, 8);

            var format = CheckSignature(buffer);
            if (format != FileFormats.Supported)
            {
                return new ReadHeaderResult
                {
                    Format = format,
                };
            }

            // Schema version
            buffer = await hash.ReadAsync(buffer, 4);

            var version = GetVersion(buffer);
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
            var headers = await GetHeaders(hash, buffer);
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
        public static async Task<XDocument> ParseContent(
            IInputStream decrypted, bool useGZip, FileHeaders headers)
        {
            if (decrypted == null) throw new ArgumentNullException("decrypted");
            if (headers == null) throw new ArgumentNullException("headers");

            var deHashed = await HashedBlockFileFormat.Read(decrypted);
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
        public static bool VerifyHeaders(FileHeaders headers, XDocument doc)
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
        public static bool VerifyHeaders(IBuffer headerHash, XDocument doc)
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

            var expected = CryptographicBuffer
                .DecodeFromBase64String(meta);

            return CryptographicBuffer.Compare(expected, headerHash);
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
        public static async Task<bool> VerifyStartBytes(IInputStream input, IBuffer startBytes)
        {
            if (input == null) throw new ArgumentNullException("input");
            if (startBytes == null) throw new ArgumentNullException("startBytes");

            var reader = new DataReader(input);
            var read = await reader.LoadAsync(startBytes.Length);
            if (read != startBytes.Length)
                return false;

            var actual = reader.ReadBuffer(startBytes.Length);
            return CryptographicBuffer.Compare(actual, startBytes);
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
        public static Task<bool> VerifyStartBytes(IInputStream input, FileHeaders headers)
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
        private static FileFormats CheckCompatibility(Version schema)
        {
            if (schema.Major < 3)
                return FileFormats.OldVersion;

            if (schema.Major > 3)
                return FileFormats.NewVersion;

            return schema.Minor > 1
                ? FileFormats.PartialSupported
                : FileFormats.Supported;
        }

        /// <summary>
        /// Gets the file format of the specified stream based on file signature.
        /// </summary>
        /// <param name="buffer">The signature bytes buffer.</param>
        /// <returns>The detected database file format.</returns>
        private static FileFormats CheckSignature(IBuffer buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            // KeePass 1.x
            var oldSignature = CryptographicBuffer
                .DecodeFromHexString("03D9A29A65FB4BB5");
            if (CryptographicBuffer.Compare(buffer, oldSignature))
                return FileFormats.KeePass1x;

            // KeePass 2.x pre-release
            var preRelease = CryptographicBuffer
                .DecodeFromHexString("03D9A29A66FB4BB5");
            if (CryptographicBuffer.Compare(buffer, preRelease))
                return FileFormats.OldVersion;

            // KeePass 2.x
            var current = CryptographicBuffer
                .DecodeFromHexString("03D9A29A67FB4BB5");
            if (!CryptographicBuffer.Compare(buffer, current))
                return FileFormats.NotSupported;

            return FileFormats.Supported;
        }

        private static void Decrypt(FileHeaders headers, XDocument doc)
        {
            var protectedStrings = doc.Descendants("Entry")
                .SelectMany(x => x.Elements("String"))
                .Select(x => x.Element("Value"))
                .Where(x =>
                {
                    var protect = x.Attribute("Protected");
                    return protect != null && (bool)protect;
                });

            IRandomGenerator generator;
            switch (headers.RandomAlgorithm)
            {
                case CrsAlgorithm.ArcFourVariant:
                    generator = new Rc4RandomGenerator(
                        headers.ProtectedStreamKey);
                    break;

                default:
                    generator = new Salsa20RandomGenerator(
                        headers.ProtectedStreamKey);
                    break;
            }

            foreach (var protectedString in protectedStrings)
            {
                var encrypted = Convert.FromBase64String(
                    protectedString.Value);
                var length = encrypted.Length;

                var padding = generator.GetRandomBytes(length);

                for (var i = 0U; i < length; i++)
                    encrypted[i] ^= padding.GetByte(i);

                protectedString.Value = Encoding.UTF8
                    .GetString(encrypted, 0, length);
            }
        }

        /// <summary>
        /// Parse the headers fields.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="buffer">The header bytes reader.</param>
        /// <returns>The file headers.</returns>
        private static async Task<FileHeaders> GetHeaders(
            IInputStream input, IBuffer buffer)
        {
            var result = new FileHeaders();

            while (true)
            {
                buffer = await input.ReadAsync(buffer, 3);
                var field = (HeaderFields)buffer.GetByte(0);
                var size = BitConverter.ToUInt16(buffer.ToArray(1, 2), 0);

                if (size > 0)
                    buffer = await input.ReadAsync(buffer, size);

                switch (field)
                {
                    case HeaderFields.EndOfHeader:
                        return result;

                    case HeaderFields.CompressionFlags:
                        result.UseGZip = buffer.GetByte(0) == 1;
                        break;

                    case HeaderFields.EncryptionIV:
                        result.EncryptionIV = buffer
                            .ToArray().AsBuffer();
                        break;

                    case HeaderFields.MasterSeed:
                        result.MasterSeed = buffer
                            .ToArray().AsBuffer();
                        break;

                    case HeaderFields.StreamStartBytes:
                        result.StartBytes = buffer
                            .ToArray().AsBuffer();
                        break;

                    case HeaderFields.TransformSeed:
                        result.TransformSeed = buffer
                            .ToArray().AsBuffer();
                        break;

                    case HeaderFields.TransformRounds:
                        result.TransformRounds = BitConverter.ToUInt64(
                            buffer.ToArray(), 0);
                        break;

                    case HeaderFields.ProtectedStreamKey:
                        result.ProtectedStreamKey = buffer
                            .ToArray().AsBuffer();
                        break;

                    case HeaderFields.InnerRandomStreamID:
                        result.RandomAlgorithm = (CrsAlgorithm)
                            BitConverter.ToUInt32(buffer.ToArray(), 0);
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the database schema version.
        /// </summary>
        /// <param name="buffer">The version bytes buffer</param>
        /// <returns>The database schema version.</returns>
        private static Version GetVersion(IBuffer buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            var bytes = buffer.ToArray(0, 4);
            var minor = BitConverter.ToUInt16(bytes, 0);
            var major = BitConverter.ToUInt16(bytes, 2);

            return new Version(major, minor);
        }
    }
}