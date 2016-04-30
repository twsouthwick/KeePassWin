using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace KeePass.Crypto
{
    public class Salsa20RandomGenerator : IRandomGenerator
    {
        private readonly Salsa20Cipher _cipher;

        public Salsa20RandomGenerator(IBuffer key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            key = HashAlgorithmProvider
                .OpenAlgorithm(HashAlgorithmNames.Sha256)
                .HashData(key);

            var iv = new byte[]
            {
                0xE8, 0x30, 0x09, 0x4B,
                0x97, 0x20, 0x5D, 0x2A
            };

            _cipher = new Salsa20Cipher(key.ToArray(), iv);
        }

        public IBuffer GetRandomBytes(int size)
        {
            var result = new byte[size];

            if (size > 0)
                _cipher.Encrypt(result, result.Length, false);

            return result.AsBuffer();
        }
    }
}