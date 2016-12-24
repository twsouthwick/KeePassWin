using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace KeePass
{
    public abstract class DatabaseCache : IDatabaseCache
    {
        protected readonly ILogger _log;

        private readonly IDatabaseFileAccess _fileAccess;
        private readonly IFilePicker _filePicker;

        public DatabaseCache(ILogger log, IDatabaseFileAccess databaseTracker, IFilePicker filePicker)
        {
            _log = log;
            _fileAccess = databaseTracker;
            _filePicker = filePicker;
        }

        public async Task<IKeePassDatabase> UnlockAsync(KeePassId id, ICredentialProvider credentialProvider)
        {
            _log.Info("Unlock {Database}", id);

            var dbFile = await _fileAccess.GetDatabaseAsync(id);

            Debug.Assert(dbFile != null);

            var credentials = await credentialProvider.GetCredentialsAsync(dbFile);

            if (credentials.Equals(default(KeePassCredentials)))
            {
                _log.Warning("Failed to retrieve credentials for {Database}", id);
                return null;
            }

            _log.Info("Successfully retrieved credentials for {Database}", id);

            return await UnlockAsync(dbFile, credentials);
        }

        public abstract Task<IKeePassDatabase> UnlockAsync(IFile dbFile, KeePassCredentials credentials);

        public async Task<IFile> AddDatabaseAsync()
        {
            _log.Info("Adding a database");
            var result = await _filePicker.GetDatabaseAsync();

            if (result == null)
            {
                return null;
            }

            if (await _fileAccess.AddDatabaseAsync(result))
            {
                return result;
            }
            else
            {
                throw new DatabaseAlreadyExistsException();
            }
        }

        public async Task<IFile> AddKeyFileAsync(IFile db)
        {
            var result = await _filePicker.GetKeyFileAsync();

            if (result == null)
            {
                return null;
            }

            await _fileAccess.AddKeyFileAsync(db, result);

            return result;
        }


        public Task<IEnumerable<IFile>> GetDatabaseFilesAsync()
        {
            _log.Info("Getting available databases");
            return _fileAccess.GetDatabasesAsync();
        }

        public Task RemoveDatabaseAsync(IFile dbFile)
        {
            _log.Info("Removing {Database}", dbFile);
            return _fileAccess.RemoveDatabaseAsync(dbFile);
        }
    }
}


