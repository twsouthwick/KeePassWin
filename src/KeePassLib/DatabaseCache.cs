using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace KeePass
{
    public abstract class DatabaseCache : IDatabaseCache
    {
        private readonly IDatabaseFileAccess _fileAccess;
        private readonly Subject<(DatabaseAction, IFile)> _databases;

        public DatabaseCache(ILogger log, IDatabaseFileAccess databaseTracker)
        {
            Log = log;
            _fileAccess = databaseTracker;
            _databases = new Subject<(DatabaseAction, IFile)>();
        }

        protected ILogger Log { get; }

        public IObservable<(DatabaseAction action, IFile file)> Databases => _databases;

        public async Task<IKeePassDatabase> UnlockAsync(KeePassId id, ICredentialProvider credentialProvider)
        {
            Log.Info("Unlock {Database}", id);

            var dbFile = await _fileAccess.GetDatabaseAsync(id);

            Debug.Assert(dbFile != null);

            var credentials = await credentialProvider.GetCredentialsAsync(dbFile);

            if (credentials.Equals(default(KeePassCredentials)))
            {
                Log.Warning("Failed to retrieve credentials for {Database}", id);
                return null;
            }

            Log.Info("Successfully retrieved credentials for {Database}", id);

            return await UnlockAsync(dbFile, credentials);
        }

        public abstract Task<IKeePassDatabase> UnlockAsync(IFile dbFile, KeePassCredentials credentials);

        public async Task<IFile> AddDatabaseAsync(IFilePicker filePicker, bool autoOpen)
        {
            Log.Info("Adding a database");
            var result = await filePicker.GetDatabaseAsync();

            if (result == null)
            {
                return null;
            }

            await _fileAccess.AddDatabaseAsync(result);

            var action = autoOpen ? DatabaseAction.Open : DatabaseAction.Add;
            _databases.OnNext((action, result));

            return result;
        }

        public async Task<IFile> AddKeyFileAsync(IFile db, IFilePicker filePicker)
        {
            var result = await filePicker.GetKeyFileAsync();

            if (result == null)
            {
                return null;
            }

            await _fileAccess.AddKeyFileAsync(db, result);

            return result;
        }

        public Task<IEnumerable<IFile>> GetDatabaseFilesAsync()
        {
            Log.Info("Getting available databases");
            return _fileAccess.GetDatabasesAsync();
        }

        public Task RemoveDatabaseAsync(IFile dbFile)
        {
            Log.Info("Removing {Database}", dbFile);
            _databases.OnNext((DatabaseAction.Remove, dbFile));
            return _fileAccess.RemoveDatabaseAsync(dbFile);
        }
    }
}
