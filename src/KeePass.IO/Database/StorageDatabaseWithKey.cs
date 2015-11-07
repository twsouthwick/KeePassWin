using Windows.Storage;

namespace KeePass.IO.Database
{
    public struct StorageDatabaseWithKey
    {
        public StorageDatabaseWithKey(IStorageFile db, IStorageFile key)
        {
            Database = db;
            Key = key;
        }

        public IStorageFile Database { get; }
        public IStorageFile Key { get; }
    }
}
