using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace KeePass.IO.Database
{
    public class DatabaseCache
    {
        public delegate void DatabaseCacheUpdatedHandler(object sender, DatabaseCacheEvent arg, StorageDatabaseWithKey database);

        private readonly DatabaseTracker _databaseTracker;

        public DatabaseCache(DatabaseTracker settings)
        {
            _databaseTracker = settings;
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
                DatabaseUpdated?.Invoke(this, DatabaseCacheEvent.Added, new StorageDatabaseWithKey(result, null));
            }
            else
            {
                DatabaseUpdated?.Invoke(this, DatabaseCacheEvent.AlreadyExists, new StorageDatabaseWithKey(result, null));
            }

            return result;
        }

        public async Task<IStorageFile> AddKeyFileAsync(string dbName)
        {
            var result = await OpenFileAsync("*");

            if (result == null)
            {
                return null;
            }

            await _databaseTracker.AddKeyFileAsync(dbName, result);

            return result;
        }

        private async Task<IStorageFile> OpenFileAsync(string extension)
        {
            var picker = new FileOpenPicker();

            picker.FileTypeFilter.Add(extension);

            return await picker.PickSingleFileAsync();
        }

        public event DatabaseCacheUpdatedHandler DatabaseUpdated;

        public Task<IEnumerable<StorageDatabaseWithKey>> GetDatabaseFilesAsync()
        {
            return _databaseTracker.GetDatabasesAsync();
        }

    }
}


