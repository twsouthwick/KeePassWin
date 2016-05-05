using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace KeePass.Crypto
{
    public class WindowsHashProvider : IHashProvider
    {
        public IHash GetSha256() => new WindowsCryptographicHash(HashAlgorithmNames.Sha256);

        public byte[] GetSha256(byte[] input) => Hash(HashAlgorithmNames.Sha256, input);

        private static byte[] Hash(string name, byte[] data)
        {
            return HashAlgorithmProvider.OpenAlgorithm(name)
                .HashData(data.AsBuffer())
                .ToArray();
        }

        public byte[] DecryptAesCbcPkcs7(byte[] seed, Stream input, byte[] encryptionIV)
        {
            var aes = SymmetricKeyAlgorithmProvider
                .OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7)
                .CreateSymmetricKey(seed.AsBuffer());

            var buffer = new byte[(int)(input.Length - input.Position)];
            var count = input.Read(buffer, 0, buffer.Length);
            var result = CryptographicEngine.Decrypt(aes, buffer.AsBuffer(), encryptionIV.AsBuffer());

            return result.ToArray();
        }

        public ICryptoEncryptor EncryptAesEcb(byte[] seed)
        {
            return new WindowCryptoEncryptor(SymmetricAlgorithmNames.AesEcb, seed);
        }

        public byte[] HexStringToBytes(string hex)
        {
            return CryptographicBuffer.DecodeFromHexString(hex).ToArray();
        }

        private class WindowCryptoEncryptor : ICryptoEncryptor
        {
            private readonly CryptographicKey _key;

            public WindowCryptoEncryptor(string name, byte[] seed)
            {
                var aes = SymmetricKeyAlgorithmProvider
                    .OpenAlgorithm(name);
                _key = aes.CreateSymmetricKey(seed.AsBuffer());
            }

            public byte[] Encrypt(byte[] data, byte[] iv)
            {
                return CryptographicEngine
                    .Encrypt(_key, data.AsBuffer(), iv?.AsBuffer())
                    .ToArray();
            }
        }

        private class WindowsCryptographicHash : IHash
        {
            private readonly CryptographicHash _hash;

            public WindowsCryptographicHash(string name)
            {
                _hash = HashAlgorithmProvider
                    .OpenAlgorithm(name)
                    .CreateHash();
            }

            public void Append(byte[] data)
            {
                _hash.Append(data.AsBuffer());
            }

            public byte[] GetValueAndReset()
            {
                return _hash.GetValueAndReset().ToArray();
            }
        }
    }
}
