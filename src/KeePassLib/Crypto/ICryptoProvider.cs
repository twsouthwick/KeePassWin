using System.IO;

namespace KeePass.Crypto
{
    public interface IHash
    {
        byte[] GetValueAndReset();
        void Append(byte[] data);
    }

    public interface ICryptoProvider
    {
        byte[] GetSha256(byte[] input);
        IHash GetSha256();
        byte[] Decrypt(Stream input, byte[] key, byte[] iv);
        byte[] Encrypt(byte[] data, byte[] key, byte[] iv);
        byte[] HexStringToBytes(string hex);
    }
}
