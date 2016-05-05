using System.IO;

namespace KeePass.Crypto
{
    public interface ICryptoEncryptor
    {
        byte[] Encrypt(byte[] data, byte[] iv);
    }

    public interface IHash
    {
        byte[] GetValueAndReset();
        void Append(byte[] data);
    }

    public interface IHashProvider
    {
        byte[] GetSha256(byte[] input);
        IHash GetSha256();
        byte[] DecryptAesCbcPkcs7(byte[] seed, Stream input, byte[] encryptionIV);
        ICryptoEncryptor EncryptAesEcb(byte[] seed);
        byte[] HexStringToBytes(string hex);
    }
}
