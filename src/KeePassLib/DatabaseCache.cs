using System.Collections.Generic;
using System.Threading.Tasks;

namespace KeePass
{
    public class DatabaseCache
    {
        public delegate void DatabaseCacheUpdatedHandler(object sender, DatabaseCacheEvent arg, IFile database);

        private readonly IDatabaseTracker _databaseTracker;
        private readonly IFilePicker _filePicker;

        public DatabaseCache(IDatabaseTracker databaseTracker, IFilePicker filePicker)
        {
            _databaseTracker = databaseTracker;
            _filePicker = filePicker;
        }

        public async Task<IFile> AddDatabaseAsync()
        {
            var result = await _filePicker.GetDatabaseAsync();

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
            var result = await _filePicker.GetKeyFileAsync();

            if (result == null)
            {
                return null;
            }

            await _databaseTracker.AddKeyFileAsync(db, result);

            return result;
        }


        public event DatabaseCacheUpdatedHandler DatabaseUpdated;

        public Task<IEnumerable<IFile>> GetDatabaseFilesAsync()
        {
            return _databaseTracker.GetDatabasesAsync();
        }

        public Task RemoveDatabaseAsync(IFile dbFile)
        {
            return _databaseTracker.RemoveDatabaseAsync(dbFile);
        }
    }
}


