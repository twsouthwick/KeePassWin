using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace KeePass.Win.Services
{
    public class FileDatabaseTracker : IDatabaseTracker
    {
        private readonly StorageItemAccessList _accessList;
        private readonly IAsyncOperation<StorageFolder> _folder;

        public FileDatabaseTracker()
        {
            _folder = ApplicationData.Current.LocalFolder.CreateFolderAsync("opened_databases", CreationCollisionOption.OpenIfExists);
            _accessList = StorageApplicationPermissions.FutureAccessList;
        }

        public async Task RemoveDatabaseAsync(IFile dbFile)
        {
            var token = dbFile.IdFromPath();

            _accessList.Remove(GetDatabaseToken(token));

            var keyToken = GetKeyToken(token);
            if (_accessList.ContainsItem(keyToken))
            {
                _accessList.Remove(keyToken);
            }

            var folder = await _folder;

            try
            {
                var file = await folder.GetFileAsync((string)token);

                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception)
            { }
        }

        public async Task<bool> AddDatabaseAsync(IFile dbFile)
        {
            var token = dbFile.IdFromPath();

            _accessList.AddOrReplace(GetDatabaseToken(token), dbFile.AsStorageItem());

            var folder = await _folder;

            // Check if file already has been created
            var files = await folder.CreateFileQuery().GetFilesAsync();

            if (files.Any(f => string.Equals(f.Name, (string)token, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            // File doesn't exist, so we will now create it. We don't need any contents in, just to be present
            await folder.CreateFileAsync((string)token, CreationCollisionOption.FailIfExists);

            return true;
        }

        public Task AddKeyFileAsync(IFile dbFile, IFile keyFile)
        {
            var token = GetKeyToken(dbFile.IdFromPath());

            _accessList.AddOrReplace(token, keyFile.AsStorageItem());

            return Task.CompletedTask;
        }

        public async Task<IFile> GetKeyFileAsync(IFile dbFile)
        {
            var token = GetKeyToken(dbFile.IdFromPath());

            if (_accessList.ContainsItem(token))
            {
                var key = await _accessList.GetFileAsync(token);

                if (!key.IsAvailable)
                {
                    _accessList.Remove(token);
                    return null;
                }

                return key.AsFile();
            }
            else
            {
                return null;
            }
        }

        public async Task<IFile> GetDatabaseAsync(KeePassId id)
        {
            var token = GetDatabaseToken(id);

            if (!_accessList.ContainsItem(token))
            {
                return null;
            }

            try
            {
                var file = await _accessList.GetFileAsync(token);

                if (file.IsAvailable)
                {
                    return file.AsFile();
                }
            }
            catch (Exception) { }

            return null;
        }

        public async Task<IEnumerable<IFile>> GetDatabasesAsync()
        {
            var folder = await _folder;
            var files = await folder.GetFilesAsync();
            var result = new List<IFile>();

            foreach (var file in files)
            {
                Guid g;

                if (Guid.TryParse(file.Name.Replace("-", ""), out g))
                {
                    var id = new KeePassId(g);
                    var dbStorageItem = await GetDatabaseAsync(id);

                    // Ensure that the item exists and the ID is consistent with the ID generator
                    if (string.Equals(dbStorageItem?.IdFromPath().ToString(), file.Name, StringComparison.Ordinal))
                    {
                        result.Add(dbStorageItem);
                    }
                    else
                    {
                        // There was a problem with the db cache
                        await file.DeleteAsync();
                    }
                }
                else
                {
                    // There was a problem parsing the name
                    await file.DeleteAsync();
                }
            }

            return result;
        }

        private string GetDatabaseToken(KeePassId token) => $"{token}.kdbx";

        private string GetKeyToken(KeePassId token) => $"{token}.key";
    }
}