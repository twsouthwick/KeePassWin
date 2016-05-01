namespace KeePass.Crypto
{
    public interface IRandomGeneratorProvider
    {
        IRandomGenerator Get(CrsAlgorithm algorithm, byte[] protectedStreamKey);
    }
}
