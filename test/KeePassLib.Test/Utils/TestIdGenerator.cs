namespace KeePass
{
    internal class TestIdGenerator : IKeePassIdGenerator
    {
        public KeePassId FromPath(string path)
        {
            return new KeePassId(path);
        }
    }
}
