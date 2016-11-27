using KeePass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace KeePassLib
{
    public class DecryptorTests
    {
        [InlineData("password-aes_rijndael_256.kdbx", "12345", false)]
        [InlineData("password-key-aes_rijndael_256.kdbx", "12345", true)]
        [InlineData("key-aes_rijndael_256.kdbx", null, true)]
        [Theory]
        public async Task Decryption(string db, string password, bool hasKey)
        {
            var kdbx = TestAssets.GetFile(db);
            var fileAccess = new TestFileAccess(kdbx);
            var credentials = new TestCredentialProvider(TestAssets.GetFile(Path.GetFileNameWithoutExtension(db)), password);
            var unlocker = new DatabaseCache(fileAccess, null);

            var result = await unlocker.UnlockAsync(kdbx.IdFromPath(), credentials);

            Assert.NotNull(result);
        }

        private sealed class TestFileAccess : IDatabaseFileAccess
        {
            private readonly IFile _db;

            public TestFileAccess(IFile db)
            {
                _db = db;
            }

            public Task<bool> AddDatabaseAsync(IFile dbFile)
            {
                throw new NotImplementedException();
            }

            public Task AddKeyFileAsync(IFile dbFile, IFile keyFile)
            {
                throw new NotImplementedException();
            }

            public Task<IFile> GetDatabaseAsync(KeePassId id)
            {
                return Task.FromResult(_db);
            }

            public Task<IEnumerable<IFile>> GetDatabasesAsync()
            {
                throw new NotImplementedException();
            }

            public Task<IFile> GetKeyFileAsync(IFile dbFile)
            {
                throw new NotImplementedException();
            }

            public Task RemoveDatabaseAsync(IFile dbFile)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class TestCredentialProvider : ICredentialProvider
        {
            private readonly IFile _keyFile;
            private readonly string _password;

            public TestCredentialProvider(IFile keyFile, string password)
            {
                _keyFile = keyFile;
                _password = password;
            }

            public Task<KeePassCredentials> GetCredentialsAsync(IFile file)
            {
                return Task.FromResult(new KeePassCredentials(_keyFile, _password));
            }
        }

        private sealed class PathFile : IFile
        {
            public PathFile(string path)
            {
                Path = path;
                Name = System.IO.Path.GetFileName(path);
            }

            public string Name { get; }

            public string Path { get; }

            public Task<Stream> OpenReadAsync()
            {
                return Task.FromResult<Stream>(File.OpenRead(Path));
            }

            public Task<Stream> OpenWriteAsync()
            {
                return Task.FromResult<Stream>(File.OpenWrite(Path));
            }
        }
    }
}
