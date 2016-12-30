using KeePass.Win.Services;
using Windows.Storage;

namespace KeePass.Win
{
    public static class StorageItemExtensions
    {
        public static IFile AsFile(this IStorageItem item) => AsFile(item as IStorageFile);

        public static IFile AsFile(this IStorageFile file)
        {
            if (file == null)
            {
                return null;
            }

            return new StorageItemFile(file);
        }

        public static IStorageItem AsStorageItem(this IFile file)
        {
            return (file as StorageItemFile)?.File;
        }
    }
}
