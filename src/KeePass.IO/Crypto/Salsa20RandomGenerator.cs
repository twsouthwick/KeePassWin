using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography.Core;

namespace KeePass.Crypto
{
    public class Salsa20RandomGenerator : IRandomGenerator
    {
        private readonly Salsa20Cipher _cipher;

        public Salsa20RandomGenerator(byte[] key)
        {
            if (key == null)
                throw new ArgumentNullException("key");


            var iv = new byte[]
            {
                0xE8, 0x30, 0x09, 0x4B,
                0x97, 0x20, 0x5D, 0x2A
            };

            _cipher = new Salsa20Cipher(GetSha256(key), iv);
        }

        private byte[] GetSha256(byte[] input)
        {
            return HashAlgorithmProvider
               .OpenAlgorithm(HashAlgorithmNames.Sha256)
               .HashData(input.AsBuffer())
               .ToArray();
        }

        public byte[] GetRandomBytes(int size)
        {
            var result = new byte[size];

            if (size > 0)
                _cipher.Encrypt(result, result.Length, false);

            return result;
        }
    }
}