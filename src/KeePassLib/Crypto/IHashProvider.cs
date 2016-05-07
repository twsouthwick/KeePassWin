using System.IO;

namespace KeePass.Crypto
{
    public interface IHash
    {
        byte[] GetValueAndReset();
        void Append(byte[] data);
    }

    public interface IHashProvider
    {
        byte[] GetSha256(byte[] input);
        IHash GetSha256();
        byte[] Decrypt(byte[] seed, Stream input, byte[] encryptionIV);
        byte[] Encrypt(byte[] seed, byte[] data, byte[] iv);
        byte[] HexStringToBytes(string hex);
    }
}
