using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using KeePass.IO.Models;
using Windows.Storage;

namespace KeePass.IO
{
    public class PasswordData
    {
        private IBuffer _keyFile;

        /// <summary>
        /// Determines if a keyfile has been registered.
        /// </summary>
        public bool HasKeyFile
        {
            get { return _keyFile != null; }
        }

        /// <summary>
        /// Determines if this instance has valid data.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return _keyFile != null ||
                    !string.IsNullOrEmpty(Password);
            }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password { get; set; }

        /// <summary>
        /// Adds the specified key file.
        /// </summary>
        /// <param name="input">The keyfile stream. If a null stream is passed in, no key file is added</param>
        public async Task AddKeyFileAsync(IStorageFile keyFile)
        {
            if (keyFile == null)
            {
                return;
            }

            using (var input = await keyFile.OpenReadAsync())
            {
                _keyFile = await LoadKeyFile(input);
            }
        }

        /// <summary>
        /// Clears the keyfile.
        /// </summary>
        public void ClearKeyfile()
        {
            _keyFile = null;
        }

        /// <summary>
        /// Gets the raw master key.
        /// </summary>
        /// <returns>The raw master key data.</returns>
        public IBuffer GetMasterKey()
        {
            var hash = HashAlgorithmProvider
                .OpenAlgorithm(HashAlgorithmNames.Sha256)
                .CreateHash();

            if (!string.IsNullOrEmpty(Password))
            {
                hash.Append(CryptographicBuffer.ConvertStringToBinary
                    (Password, BinaryStringEncoding.Utf8));

                var buffer = hash.GetValueAndReset();
                hash.Append(buffer);
            }

            if (_keyFile != null)
                hash.Append(_keyFile);

            return hash.GetValueAndReset();
        }

        /// <summary>
        /// Gets the transformed master key.
        /// </summary>
        /// <param name="headers">The database file headers.</param>
        /// <returns>The transformation operation.</returns>
        public IAsyncOperationWithProgress<IBuffer, uint>
            GetMasterKey(FileHeaders headers)
        {
            if (headers == null)
                throw new ArgumentNullException("headers");

            return GetMasterKey(headers.TransformSeed,
                headers.TransformRounds);
        }

        /// <summary>
        /// Gets the transformed master key.
        /// </summary>
        /// <param name="seed">The transformation seed.</param>
        /// <param name="rounds">The number of transformation rounds.</param>
        /// <returns>The transformation operation.</returns>
        public IAsyncOperationWithProgress<IBuffer, uint>
            GetMasterKey(IBuffer seed, ulong rounds)
        {
            return AsyncInfo.Run<IBuffer, uint>(
                (token, progress) => Task.Run(() =>
                {
                    var transforms = 0UL;
                    var master = GetMasterKey();

                    // AES - ECB
                    var aes = SymmetricKeyAlgorithmProvider
                        .OpenAlgorithm(SymmetricAlgorithmNames.AesEcb);
                    var key = aes.CreateSymmetricKey(seed);


                    while (true)
                    {
                        // Report progress
                        token.ThrowIfCancellationRequested();
                        progress.Report((uint)Math.Round(
                            transforms * 100F / rounds));

                        for (var i = 0; i < 1000; i++)
                        {
                            // Transform master key
                            master = CryptographicEngine
                                .Encrypt(key, master, null);

                            transforms++;
                            if (transforms < rounds)
                                continue;

                            // Completed
                            progress.Report(100);
                            master = HashAlgorithmProvider
                                .OpenAlgorithm(HashAlgorithmNames.Sha256)
                                .HashData(master);

                            return master;
                        }
                    }
                }, token));
        }

        /// <summary>
        /// Gets the SHA 256 hash of the specified file.
        /// </summary>
        /// <param name="input">The keyfile stream.</param>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The hash of the keyfile.</returns>
        private static async Task<IBuffer> GetFileHash(
            IInputStream input, IBuffer buffer)
        {
            var sha = HashAlgorithmProvider
                .OpenAlgorithm(HashAlgorithmNames.Sha256)
                .CreateHash();

            while (true)
            {
                buffer = await input.ReadAsync(
                    buffer, buffer.Capacity);

                if (buffer.Length == 0)
                    break;

                sha.Append(buffer);
            }

            return sha.GetValueAndReset();
        }

        /// <summary>
        /// Determines whether the specified string is a valid hex string.
        /// </summary>
        /// <param name="hex">The hexadecimal string.</param>
        /// <returns><c>true</c> if the string is a valid hex string; otherwise, <c>false</c>.</returns>
        private static bool IsHexString(string hex)
        {
            return hex.All(ch =>
                ((ch >= '0') && (ch <= '9')) ||
                    ((ch >= 'a') && (ch <= 'z')) ||
                    ((ch >= 'A') && (ch <= 'Z')));
        }

        /// <summary>
        /// Loads the specified key file.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// The <paramref name="input"/> parameter cannot be <c>null</c>.
        /// </exception>
        private static async Task<IBuffer> LoadKeyFile(IRandomAccessStream input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            var buffer = WindowsRuntimeBuffer.Create(1024);

            switch (input.Size)
            {
                case 32: // Binary key file
                    return await input.ReadAsync(buffer, 32);

                case 64: // Hex text key file
                    buffer = await input.ReadAsync(buffer, 64);
                    var hex = CryptographicBuffer.ConvertBinaryToString(
                        BinaryStringEncoding.Utf8, buffer);

                    if (IsHexString(hex))
                        return CryptographicBuffer.DecodeFromHexString(hex);
                    break;
            }

            // XML
            input.Seek(0);
            var xml = LoadXmlKeyFile(input);
            if (xml != null)
                return xml;

            // Random keyfile
            input.Seek(0);
            return await GetFileHash(input, buffer);
        }

        /// <summary>
        /// Loads the specified XML key file.
        /// </summary>
        /// <param name="input">The keyfile stream.</param>
        /// <returns>The binary data buffer, or <c>null</c> if not a valid XML keyfile.</returns>
        private static IBuffer LoadXmlKeyFile(IInputStream input)
        {
            try
            {
                var doc = XDocument.Load(input.AsStreamForRead());

                // Root
                var root = doc.Root;
                if (root == null || root.Name != "KeyFile")
                    return null;

                // Key
                var key = root.Element("Key");
                if (key == null)
                    return null;

                // Data
                var data = key.Element("Data");
                if (data == null)
                    return null;

                try
                {
                    return CryptographicBuffer
                        .DecodeFromBase64String(data.Value);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            catch (XmlException)
            {
                return null;
            }
        }
    }
}