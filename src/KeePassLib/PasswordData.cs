using KeePass.Crypto;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace KeePass
{
    public class PasswordData
    {
        private readonly ICryptoProvider _hashProvider;

        private byte[] _keyFile;

        public PasswordData(ICryptoProvider hashProvider)
        {
            _hashProvider = hashProvider;
        }

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
        public bool IsValid => _keyFile != null || !string.IsNullOrEmpty(Password);

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
        public async Task AddKeyFileAsync(IFile keyFile)
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
        public byte[] GetMasterKey()
        {
            var hash = _hashProvider.GetSha256();

            if (!string.IsNullOrEmpty(Password))
            {
                hash.Append(Encoding.UTF8.GetBytes(Password));

                var buffer = hash.GetValueAndReset();
                hash.Append(buffer);
            }

            if (_keyFile != null)
            {
                hash.Append(_keyFile);
            }

            return hash.GetValueAndReset();
        }

        /// <summary>
        /// Gets the transformed master key.
        /// </summary>
        /// <param name="headers">The database file headers.</param>
        /// <returns>The transformation operation.</returns>
        public Task<byte[]> GetMasterKey(FileHeaders headers)
        {
            if (headers == null)
                throw new ArgumentNullException("headers");

            return GetMasterKey(headers.TransformSeed, headers.TransformRounds);
        }

        /// <summary>
        /// Gets the transformed master key.
        /// </summary>
        /// <param name="seed">The transformation seed.</param>
        /// <param name="rounds">The number of transformation rounds.</param>
        /// <returns>The transformation operation.</returns>
        public Task<byte[]> GetMasterKey(byte[] seed, ulong rounds)
        {
            return Task.Run(() =>
            {
                var transforms = 0UL;
                var master = GetMasterKey();

                while (true)
                {
                    for (var i = 0; i < 1000; i++)
                    {
                        // Transform master key
                        master = _hashProvider.Encrypt(master, seed, null);

                        transforms++;

                        if (transforms < rounds)
                            continue;

                        return _hashProvider.GetSha256(master);
                    }
                }
            });
        }

        /// <summary>
        /// Gets the SHA 256 hash of the specified file.
        /// </summary>
        /// <param name="input">The keyfile stream.</param>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The hash of the keyfile.</returns>
        private async Task<byte[]> GetFileHash(Stream input)
        {
            var sha = _hashProvider.GetSha256();

            while (true)
            {
                var buffer = await input.ReadAsync(1024);

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
        private async Task<byte[]> LoadKeyFile(Stream input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            switch (input.Length)
            {
                case 32: // Binary key file
                    return await input.ReadAsync(32);

                case 64: // Hex text key file
                    var buffer = await input.ReadAsync(64);
                    var hex = Encoding.UTF8.GetString(buffer);

                    if (IsHexString(hex))
                    {
                        return _hashProvider.HexStringToBytes(hex);
                    }

                    break;
            }

            // XML
            input.Seek(0, SeekOrigin.Begin);
            var xml = LoadXmlKeyFile(input);
            if (xml != null)
                return xml;

            // Random keyfile
            input.Seek(0, SeekOrigin.Begin);
            return await GetFileHash(input);
        }

        /// <summary>
        /// Loads the specified XML key file.
        /// </summary>
        /// <param name="input">The keyfile stream.</param>
        /// <returns>The binary data buffer, or <c>null</c> if not a valid XML keyfile.</returns>
        private static byte[] LoadXmlKeyFile(Stream input)
        {
            try
            {
                var doc = XDocument.Load(input);

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
                    return Convert.FromBase64String(data.Value);
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