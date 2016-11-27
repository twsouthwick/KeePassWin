using KeePassLib;
using KeePassLib.Keys;
using KeePassLib.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace KeePass
{
    public class DatabaseCache : IDatabaseCache
    {
        private readonly IDatabaseFileAccess _fileAccess;
        private readonly IFilePicker _filePicker;

        public DatabaseCache(IDatabaseFileAccess databaseTracker, IFilePicker filePicker)
        {
            _fileAccess = databaseTracker;
            _filePicker = filePicker;
        }

        public async Task<IKeePassDatabase> UnlockAsync(KeePassId id, ICredentialProvider credentialProvider)
        {
            var dbFile = await _fileAccess.GetDatabaseAsync(id);

            Debug.Assert(dbFile != null);

            var credentials = await credentialProvider.GetCredentialsAsync(dbFile);

            if (credentials.Equals(default(KeePassCredentials)))
            {
                return null;
            }

            return await UnlockAsync(dbFile, credentials);
        }

        public async Task<IKeePassDatabase> UnlockAsync(IFile dbFile, KeePassCredentials credentials)
        {
            try
            {
                var compositeKey = new CompositeKey();

                if (credentials.Password != null)
                {
                    compositeKey.AddUserKey(new KcpPassword(credentials.Password));
                }

                if (credentials.KeyFile != null)
                {
                    compositeKey.AddUserKey(new KcpKeyFile(await credentials.KeyFile.ReadFileBytesAsync()));
                }

                var db = new PwDatabase
                {
                    MasterKey = compositeKey
                };

                var kdbx = new KdbxFile(db);

                using (var fs = await dbFile.OpenReadAsync())
                {
                    await Task.Run(() =>
                    {
                        kdbx.Load(fs, KdbxFormat.Default, null);
                    });

                    return new KdbxDatabase(dbFile, db, dbFile.IdFromPath());
                }
            }
            catch (DatabaseUnlockException)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);

                throw new DatabaseUnlockException(e);
            }
        }

        public async Task<IFile> AddDatabaseAsync()
        {
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
            return _fileAccess.GetDatabasesAsync();
        }

        public Task RemoveDatabaseAsync(IFile dbFile)
        {
            return _fileAccess.RemoveDatabaseAsync(dbFile);
        }
    }
}


