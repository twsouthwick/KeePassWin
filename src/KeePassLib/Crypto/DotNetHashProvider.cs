using KeePass.Crypto;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace KeePass
{
    public class DotNetHashProvider : ICryptoProvider
    {
        public byte[] Decrypt(Stream input, byte[] key, byte[] iv)
        {
            using (var rijndael = Aes.Create())
            {
                rijndael.KeySize = 256;
                rijndael.Key = key;
                rijndael.Mode = CipherMode.CBC;
                rijndael.Padding = PaddingMode.PKCS7;

                if (iv != null)
                {
                    rijndael.IV = iv;
                }

                var transform = rijndael.CreateDecryptor();

                using (var decryptStream = new CryptoStream(input, transform, CryptoStreamMode.Read))
                using (var ms = new MemoryStream())
                {
                    decryptStream.CopyTo(ms);
                    decryptStream.Flush();
                    ms.Position = 0;
                    return ms.ToArray();
                }
            }
        }

        public byte[] Encrypt(byte[] input, byte[] key, byte[] iv)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.KeySize = 256;
                aesAlg.Key = key;
                aesAlg.BlockSize = 128;
                aesAlg.Mode = CipherMode.ECB;
                aesAlg.Padding = PaddingMode.None;

                if (iv != null)
                {
                    aesAlg.IV = iv;
                }

                ICryptoTransform encryptor = aesAlg.CreateEncryptor();
                return encryptor.TransformFinalBlock(input, 0, input.Length);
            }
        }

        public IHash GetSha256()
        {
            return new DotNet256Hash();
        }

        public byte[] GetSha256(byte[] input)
        {
            var hasher = GetSha256();
            hasher.Append(input);
            return hasher.GetValueAndReset();
        }

        public byte[] HexStringToBytes(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        private class DotNet256Hash : IHash
        {
            private readonly MemoryStream _data = new MemoryStream();

            public DotNet256Hash()
            {
            }

            public void Append(byte[] data)
            {
                _data.Write(data, 0, data.Length);
            }

            public byte[] GetValueAndReset()
            {
                using (var hash = SHA256.Create())
                {
                    _data.Position = 0;
                    var hashValue = hash.ComputeHash(_data);

                    // Clear memory stream
                    _data.SetLength(0);

                    return hashValue;
                }

            }
        }
    }
}
