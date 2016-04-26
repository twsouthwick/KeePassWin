using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace KeePass.IO.Database
{
    public class DatabaseCache
    {
        public delegate void DatabaseCacheUpdatedHandler(object sender, DatabaseCacheEvent arg, IStorageFile database);

        private readonly IDatabaseTracker _databaseTracker;

        public DatabaseCache(IDatabaseTracker databaseTracker)
        {
            _databaseTracker = databaseTracker;
        }

        public async Task<IStorageFile> AddDatabaseAsync()
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

        public async Task<IStorageFile> AddKeyFileAsync(IStorageFile db)
        {
            var result = await OpenFileAsync("*");

            if (result == null)
            {
                return null;
            }

            await _databaseTracker.AddKeyFileAsync(db, result);

            return result;
        }

        private async Task<IStorageFile> OpenFileAsync(string extension)
        {
            var picker = new FileOpenPicker();

            picker.FileTypeFilter.Add(extension);

            return await picker.PickSingleFileAsync();
        }

        public event DatabaseCacheUpdatedHandler DatabaseUpdated;

        public Task<IEnumerable<IStorageFile>> GetDatabaseFilesAsync()
        {
            return _databaseTracker.GetDatabasesAsync();
        }

    }
}


