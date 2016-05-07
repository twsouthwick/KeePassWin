using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace KeePass.Crypto
{
    public class WindowsHashProvider : ICryptoProvider
    {
        public IHash GetSha256() => new WindowsCryptographicHash(HashAlgorithmNames.Sha256);

        public byte[] GetSha256(byte[] input) => Hash(HashAlgorithmNames.Sha256, input);

        private static byte[] Hash(string name, byte[] data)
        {
            return HashAlgorithmProvider.OpenAlgorithm(name)
                .HashData(data.AsBuffer())
                .ToArray();
        }

        public byte[] Decrypt(Stream input, byte[] seed, byte[] encryptionIV)
        {
            var alg = SymmetricKeyAlgorithmProvider
                .OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);
            var aes = alg
                .CreateSymmetricKey(seed.AsBuffer());


            var buffer = new byte[(int)(input.Length - input.Position)];
            var count = input.Read(buffer, 0, buffer.Length);
            var result = CryptographicEngine.Decrypt(aes, buffer.AsBuffer(), encryptionIV.AsBuffer());

            return result.ToArray();
        }

        public byte[] Encrypt(byte[] data, byte[] seed, byte[] iv)
        {
            var aes = SymmetricKeyAlgorithmProvider
                .OpenAlgorithm(SymmetricAlgorithmNames.AesEcb);
            var key = aes.CreateSymmetricKey(seed.AsBuffer());

            
            var blockLength = aes.BlockLength;
            var keySize = key.KeySize;
            return CryptographicEngine
                .Encrypt(key, data.AsBuffer(), iv?.AsBuffer())
                .ToArray();
        }

        public byte[] HexStringToBytes(string hex)
        {
            return CryptographicBuffer.DecodeFromHexString(hex).ToArray();
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
