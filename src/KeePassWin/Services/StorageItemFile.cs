using System;
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

        public async Task<Stream> OpenWriteAsync()
        {
            CachedFileManager.DeferUpdates(File);
            var fs = await File.OpenAsync(FileAccessMode.ReadWrite);

            // Important to ensure the file is overwritten
            fs.Size = 0;

            return new CachedFileUpdateStream(File, fs.AsStreamForWrite());
        }

        private class CachedFileUpdateStream : Stream
        {
            private readonly IStorageFile _file;
            private readonly Stream _stream;

            private bool _completed;

            public CachedFileUpdateStream(IStorageFile file, Stream stream)
            {
                _file = file;
                _stream = stream;
            }

            public override bool CanRead => _stream.CanRead;

            public override bool CanSeek => _stream.CanSeek;

            public override bool CanWrite => _stream.CanWrite;

            public override long Length => _stream.Length;

            public override long Position
            {
                get { return _stream.Position; }
                set { _stream.Position = value; }
            }

            public override void Flush() => _stream.Flush();

            protected override async void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (!_completed)
                {
                    _completed = true;
                    await CachedFileManager.CompleteUpdatesAsync(_file);
                }
            }

            public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);

            public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);

            public override void SetLength(long value) => _stream.SetLength(value);

            public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);
        }
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
