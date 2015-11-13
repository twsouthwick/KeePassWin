using KeePass.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace KeePass.IO.Database
{
    public class DatabaseTracker
    {
        private readonly StorageItemAccessList _accessList;
        private readonly IAsyncOperation<StorageFolder> _folder;

        public DatabaseTracker()
        {
            _folder = ApplicationData.Current.LocalFolder.CreateFolderAsync("db", CreationCollisionOption.OpenIfExists);
            _accessList = StorageApplicationPermissions.FutureAccessList;
        }

        public async Task<bool> AddDatabaseAsync(IStorageFile dbFile)
        {
            var token = KeePassId.FromPath(dbFile);

            _accessList.AddOrReplace(GetDatabaseToken(token), dbFile);

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

        public Task AddKeyFileAsync(IStorageFile dbFile, IStorageFile keyFile)
        {
            var token = GetKeyToken(KeePassId.FromPath(dbFile));

            _accessList.AddOrReplace(token, keyFile);

            return Task.CompletedTask;
        }

        public async Task<IStorageFile> GetKeyFileAsync(IStorageFile dbFile)
        {
            var token = GetKeyToken(KeePassId.FromPath(dbFile));

            if (_accessList.ContainsItem(token))
            {
                var key = await _accessList.GetFileAsync(token);

                if (!key.IsAvailable)
                {
                    _accessList.Remove(token);
                    return null;
                }

                return key;
            }
            {
                return null;
            }
        }

        public async Task<IEnumerable<IStorageFile>> GetDatabasesAsync()
        {
            var folder = await _folder;
            var files = await folder.GetFilesAsync();
            var result = new List<IStorageFile>();

            foreach (var file in files)
            {
                try
                {
                    var dbStorageItem = await _accessList.GetFileAsync(GetDatabaseToken(file.Name));

                    result.Add(dbStorageItem);
                }
                catch (ArgumentException)
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
