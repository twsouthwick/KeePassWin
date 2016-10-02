using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace KeePass
{
    public class StorageItemFile : IFile
    {
        public StorageItemFile(IStorageFile file)
        {
            File = file;
        }

        public IStorageFile File { get; }

        public string Path => File.Path;

        public string Name => File.Name;

        public Task<Stream> OpenReadAsync() => File.OpenStreamForReadAsync();

        public Task<Stream> OpenWriteAsync() => File.OpenStreamForWriteAsync();
    }

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
