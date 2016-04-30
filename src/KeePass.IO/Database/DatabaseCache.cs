using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace KeePass
{
    public class DatabaseCache
    {
        public delegate void DatabaseCacheUpdatedHandler(object sender, DatabaseCacheEvent arg, IFile database);

        private readonly IDatabaseTracker _databaseTracker;

        public DatabaseCache(IDatabaseTracker databaseTracker)
        {
            _databaseTracker = databaseTracker;
        }

        public async Task<IFile> AddDatabaseAsync()
        {
            var result = await OpenFileAsync(".kdbx");

            if (result == null)
            {
                return null;
            }

            if (await _databaseTracker.AddDatabaseAsync(result))
            {
                DatabaseUpdated?.Invoke(this, DatabaseCacheEvent.Added, result);
            }
            else
            {
                DatabaseUpdated?.Invoke(this, DatabaseCacheEvent.AlreadyExists, result);
            }

            return result;
        }

        public async Task<IFile> AddKeyFileAsync(IFile db)
        {
            var result = await OpenFileAsync("*");

            if (result == null)
            {
                return null;
            }

            await _databaseTracker.AddKeyFileAsync(db, result);

            return result;
        }

        private async Task<IFile> OpenFileAsync(string extension)
        {
            var picker = new FileOpenPicker();

            picker.FileTypeFilter.Add(extension);

            return (await picker.PickSingleFileAsync()).AsFile();
        }

        public event DatabaseCacheUpdatedHandler DatabaseUpdated;

        public Task<IEnumerable<IFile>> GetDatabaseFilesAsync()
        {
            return _databaseTracker.GetDatabasesAsync();
        }

    }
}


