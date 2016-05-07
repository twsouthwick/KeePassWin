using KeePass.Crypto;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace KeePassLib
{
    internal class DotNetHashProvider : ICryptoProvider
    {
        public byte[] Decrypt(Stream input, byte[] key, byte[] iv)
        {
            using (var rijndael = new RijndaelManaged
            {
                KeySize = 256,
                Key = key,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            })
            {
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
            using (var aesAlg = new AesManaged
            {
                KeySize = 256,
                Key = key,
                BlockSize = 128,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.None
            })
            {

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
            return new DotNetHash(HashAlgorithmName.SHA256);
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

        private class DotNetHash : IHash
        {
            private readonly MemoryStream _data = new MemoryStream();
            private readonly string _name;

            public DotNetHash(HashAlgorithmName name)
            {
                _name = name.Name;
            }

            public void Append(byte[] data)
            {
                _data.Write(data, 0, data.Length);
            }

            public byte[] GetValueAndReset()
            {
                using (var hash = HashAlgorithm.Create(_name))
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
