namespace KeePass.Crypto
{
    public interface IHashProvider
    {
        byte[] GetSha256(byte[] input);
    }
}
