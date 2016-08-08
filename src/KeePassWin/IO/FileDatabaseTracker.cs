using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace KeePass
{
    public class FileDatabaseTracker : IDatabaseTracker
    {
        private readonly StorageItemAccessList _accessList;
        private readonly IAsyncOperation<StorageFolder> _folder;
        private readonly IKeePassIdGenerator _idGenerator;

        public FileDatabaseTracker(IKeePassIdGenerator idGenerator)
        {
            _folder = ApplicationData.Current.LocalFolder.CreateFolderAsync("opened_databases", CreationCollisionOption.OpenIfExists);
            _accessList = StorageApplicationPermissions.FutureAccessList;
            _idGenerator = idGenerator;
        }

        public async Task RemoveDatabaseAsync(IFile dbFile)
        {
            var token = _idGenerator.FromPath(dbFile.Path);

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
            var token = _idGenerator.FromPath(dbFile.Path);

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
            var token = GetKeyToken(_idGenerator.FromPath(dbFile.Path));

            _accessList.AddOrReplace(token, keyFile.AsStorageItem());

            return Task.CompletedTask;
        }

        public async Task<IFile> GetKeyFileAsync(IFile dbFile)
        {
            var token = GetKeyToken(_idGenerator.FromPath(dbFile.Path));

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
            try
            {
                var file = await _accessList.GetFileAsync(GetDatabaseToken(id));

                if (file.IsAvailable)
                {
                    return file.AsFile();
                }
            }
            catch (ArgumentException) { }

            return null;
        }

        public async Task<IEnumerable<IFile>> GetDatabasesAsync()
        {
            var folder = await _folder;
            var files = await folder.GetFilesAsync();
            var result = new List<IFile>();

            foreach (var file in files)
            {
                var dbStorageItem = await GetDatabaseAsync(file.Name);

                // Ensure that the item exists and the ID is consistent with the ID generator
                if (dbStorageItem != null && string.Equals((string)_idGenerator.FromPath(dbStorageItem.Path), file.Name, StringComparison.Ordinal))
                {
                    result.Add(dbStorageItem);
                }
                else
                {
                    // There was a problem with the db cache
                    await file.DeleteAsync();
                }
            }

            return result;
        }

        private string GetDatabaseToken(KeePassId token) => $"{token}.kdbx";

        private string GetKeyToken(KeePassId token) => $"{token}.key";
    }
}