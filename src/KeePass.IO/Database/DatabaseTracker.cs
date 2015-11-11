using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.AccessCache;
using System.Linq;

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
            var token = GetFileToken(dbFile);

            _accessList.AddOrReplace(GetDatabaseToken(token), dbFile);

            var folder = await _folder;

            // Check if file already has been created
            var files = await folder.CreateFileQuery().GetFilesAsync();

            if (files.Any(f => string.Equals(f.Name, token, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            // File doesn't exist, so we will now create it
            var file = await folder.CreateFileAsync(token, CreationCollisionOption.FailIfExists);

            using (var randomAccessStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            using (var stream = randomAccessStream.AsStream())
            using (var writer = new StreamWriter(stream))
            {
                var entry = new KeyDatabaseJson { DatabasePath = dbFile.Path };

                await writer.WriteAsync(entry.Stringify());
            }

            return true;
        }

        public Task AddKeyFileAsync(IStorageFile dbFile, IStorageFile keyFile)
        {
            var token = GetKeyToken(GetFileToken(dbFile));

            _accessList.AddOrReplace(token, keyFile);

            return Task.CompletedTask;
        }

        public async Task<IStorageFile> GetKeyFileAsync(IStorageFile dbFile)
        {
            var token = GetKeyToken(GetFileToken(dbFile));

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

        public async Task<IEnumerable<StorageDatabaseWithKey>> GetDatabasesAsync()
        {
            var folder = await _folder;
            var files = await folder.GetFilesAsync();
            var result = new List<StorageDatabaseWithKey>();

            foreach (var file in files)
            {
                try
                {
                    var data = await KeyDatabaseJson.ParseAsync(await file.OpenStreamForReadAsync());

                    var dbStorageItem = string.IsNullOrEmpty(data.DatabasePath) ? null : await _accessList.GetFileAsync(GetDatabaseToken(file.Name));
                    var keyStorageItem = string.IsNullOrEmpty(data.KeyPath) ? null : await _accessList.GetFileAsync(GetKeyToken(file.Name));

                    Debug.Assert(dbStorageItem != null);

                    result.Add(new StorageDatabaseWithKey(dbStorageItem, keyStorageItem));
                }
                catch (InvalidDatabaseFileException)
                {
                    // There was a problem with the db cache
                    await file.DeleteAsync();
                }
            }

            return result;
        }

        private string GetDatabaseToken(string token) => $"{token}.kdbx";

        private string GetKeyToken(string token) => $"{token}.key";

        /// <summary>
        /// Generate a MD5 hash of the file name to ensure the tokens are always the same
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private string GetFileToken(IStorageFile file)
        {
            var algorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var buffer = CryptographicBuffer.ConvertStringToBinary(file.Path, BinaryStringEncoding.Utf16BE);
            var hash = algorithm.CreateHash();

            hash.Append(buffer);

            var result = hash.GetValueAndReset();

            return CryptographicBuffer.EncodeToHexString(result);
        }

        private class KeyDatabaseJson
        {
            public string DatabasePath { get; set; }

            public string KeyPath { get; set; }

            public static async Task<KeyDatabaseJson> ParseAsync(Stream stream)
            {
                using (var reader = new StreamReader(stream))
                {
                    if (reader.EndOfStream)
                    {
                        throw new InvalidDatabaseFileException();
                    }

                    var databasePath = await reader.ReadLineAsync();
                    var keyPath = reader.EndOfStream ? string.Empty : await reader.ReadLineAsync();

                    if (string.IsNullOrEmpty(databasePath))
                    {
                        throw new InvalidDatabaseFileException();
                    }

                    VerifyPath(databasePath);
                    VerifyPath(keyPath);

                    return new KeyDatabaseJson
                    {
                        DatabasePath = databasePath,
                        KeyPath = keyPath
                    };
                }
            }

            private static void VerifyPath(string path)
            {
                // TODO: We don't need a real path here right now...
            }

            public override string ToString()
            {
                var sb = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(DatabasePath)) sb.AppendLine(DatabasePath);
                if (!string.IsNullOrWhiteSpace(KeyPath)) sb.AppendLine(KeyPath);

                return sb.ToString();
            }

            public string Stringify()
            {
                return new JsonObject
                {
                    { nameof(DatabasePath), CreateStringValue(DatabasePath) },
                    { nameof(KeyPath), CreateStringValue(KeyPath) },
                }.Stringify();
            }

            private JsonValue CreateStringValue(string str) => str == null ? JsonValue.CreateNullValue() : JsonValue.CreateStringValue(str);
        }

        private class InvalidDatabaseFileException : Exception { }
    }

}
