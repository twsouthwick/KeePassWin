using System;

namespace KeePass.Crypto
{
    public class Salsa20RandomGenerator : IRandomGenerator
    {
        private readonly Salsa20Cipher _cipher;

        public Salsa20RandomGenerator(ICryptoProvider hashProvider, byte[] key)
        {
            if (hashProvider == null)
            {
                throw new ArgumentNullException(nameof(hashProvider));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var iv = new byte[]
            {
                0xE8, 0x30, 0x09, 0x4B,
                0x97, 0x20, 0x5D, 0x2A
            };

            _cipher = new Salsa20Cipher(hashProvider.GetSha256(key), iv);
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