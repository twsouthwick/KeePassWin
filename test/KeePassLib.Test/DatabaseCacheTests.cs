using NSubstitute;
using System;
using System.Threading.Tasks;
using Xunit;

namespace KeePass.Tests
{
    public class DatabaseCacheTests
    {
        [Fact]
        public async Task RemoveDatabaseTestAsync()
        {
            // Arrange
            var access = Substitute.For<IDatabaseFileAccess>();
            var log = Substitute.For<ILogger>();
            var cache = Substitute.For<DatabaseCache>(log, access, null);
            var file = Substitute.For<IFile>();

            // Act
            await cache.RemoveDatabaseAsync(file);

            // Assert
            await access.Received(1).RemoveDatabaseAsync(file);
        }

        [Fact]
        public async Task GetDatabaseTestAsync()
        {
            // Arrange
            var access = Substitute.For<IDatabaseFileAccess>();
            var log = Substitute.For<ILogger>();
            var cache = Substitute.For<DatabaseCache>(log, access, null);

            // Act
            await cache.GetDatabaseFilesAsync();

            // Assert
            await access.Received(1).GetDatabasesAsync();
        }

        [Fact]
        public async Task AddKeyFileTestNullAsync()
        {
            // Arrange
            var access = Substitute.For<IDatabaseFileAccess>();
            var log = Substitute.For<ILogger>();
            var filePicker = Substitute.For<IFilePicker>();
            var cache = Substitute.For<DatabaseCache>(log, access, filePicker);

            filePicker.GetKeyFileAsync().Returns(Task.FromResult<IFile>(null));
            var file = Substitute.For<IFile>();

            // Act
            var result = await cache.AddKeyFileAsync(file);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddKeyFileTestAsync()
        {
            // Arrange
            var access = Substitute.For<IDatabaseFileAccess>();
            var log = Substitute.For<ILogger>();
            var filePicker = Substitute.For<IFilePicker>();
            var cache = Substitute.For<DatabaseCache>(log, access, filePicker);

            var db = Substitute.For<IFile>();

            var keyfile = Substitute.For<IFile>();
            filePicker.GetKeyFileAsync().Returns(Task.FromResult(keyfile));

            // Act
            var result = await cache.AddKeyFileAsync(db);

            // Assert
            Assert.Same(keyfile, result);
            await access.Received(1).AddKeyFileAsync(db, keyfile);
        }

        [Fact]
        public async Task AddDatabaseTestNullAsync()
        {
            // Arrange
            var access = Substitute.For<IDatabaseFileAccess>();
            var log = Substitute.For<ILogger>();
            var filePicker = Substitute.For<IFilePicker>();
            var cache = Substitute.For<DatabaseCache>(log, access, filePicker);

            filePicker.GetDatabaseAsync().Returns(Task.FromResult<IFile>(null));

            // Act
            var result = await cache.AddDatabaseAsync();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddDatabaseTestTrueAsync()
        {
            // Arrange
            var log = Substitute.For<ILogger>();

            var filePicker = Substitute.For<IFilePicker>();
            var db = Substitute.For<IFile>();
            filePicker.GetDatabaseAsync().Returns(Task.FromResult(db));

            var access = Substitute.For<IDatabaseFileAccess>();
            access.AddDatabaseAsync(db).Returns(Task.FromResult(true));

            var cache = Substitute.For<DatabaseCache>(log, access, filePicker);

            // Act
            var result = await cache.AddDatabaseAsync();

            // Assert
            Assert.Equal(db, result);
            await access.Received(1).AddDatabaseAsync(db);
        }

        [Fact]
        public async Task AddDatabaseTestFalseAsync()
        {
            // Arrange
            var log = Substitute.For<ILogger>();

            var filePicker = Substitute.For<IFilePicker>();
            var db = Substitute.For<IFile>();
            filePicker.GetDatabaseAsync().Returns(Task.FromResult(db));

            var access = Substitute.For<IDatabaseFileAccess>();
            access.AddDatabaseAsync(db).Returns(Task.FromResult(false));

            var cache = Substitute.For<DatabaseCache>(log, access, filePicker);

            // Act
            await Assert.ThrowsAsync<DatabaseAlreadyExistsException>(async () => await cache.AddDatabaseAsync());

            // Assert
            await access.Received(1).AddDatabaseAsync(db);
        }

        [Fact]
        public async Task UnlockTestAsync()
        {
            // Arrange
            var id = new KeePassId(Guid.NewGuid());
            var log = Substitute.For<ILogger>();

            var filePicker = Substitute.For<IFilePicker>();
            var db = Substitute.For<IFile>();
            filePicker.GetDatabaseAsync().Returns(Task.FromResult(db));

            var access = Substitute.For<IDatabaseFileAccess>();
            access.GetDatabaseAsync(id).Returns(db);

            var credentialProvider = Substitute.For<ICredentialProvider>();
            var credentials = new KeePassCredentials(db, "pass");
            credentialProvider.GetCredentialsAsync(db).Returns(credentials);

            var cache = Substitute.ForPartsOf<DatabaseCache>(log, access, filePicker);
            var kpDb = Substitute.For<IKeePassDatabase>();
            cache.UnlockAsync(db, Arg.Any<KeePassCredentials>()).Returns(kpDb);

            // Act
            var result = await cache.UnlockAsync(id, credentialProvider);

            // Assert
            Assert.Equal(kpDb, result);
            await access.Received(1).GetDatabaseAsync(id);
            await credentialProvider.Received(1).GetCredentialsAsync(db);
        }

        [Fact]
        public async Task UnlockTestNoCredentialsAsync()
        {
            // Arrange
            var id = new KeePassId(Guid.NewGuid());
            var log = Substitute.For<ILogger>();

            var filePicker = Substitute.For<IFilePicker>();
            var db = Substitute.For<IFile>();
            filePicker.GetDatabaseAsync().Returns(Task.FromResult(db));

            var access = Substitute.For<IDatabaseFileAccess>();
            access.GetDatabaseAsync(id).Returns(db);

            var credentialProvider = Substitute.For<ICredentialProvider>();

            var cache = Substitute.ForPartsOf<DatabaseCache>(log, access, filePicker);

            // Act
            var result = await cache.UnlockAsync(id, credentialProvider);

            // Assert
            Assert.Null(result);
            await access.Received(1).GetDatabaseAsync(id);
            await credentialProvider.Received(1).GetCredentialsAsync(db);
        }
    }
}
