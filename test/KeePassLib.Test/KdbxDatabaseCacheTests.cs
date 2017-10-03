using KeePass;
using NSubstitute;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace KeePassLib
{
    public class KdbxDatabaseCacheTests
    {
        [InlineData("password-aes_rijndael_256.kdbx", "12345", false)]
        [InlineData("password-key-aes_rijndael_256.kdbx", "12345", true)]
        [InlineData("key-aes_rijndael_256.kdbx", null, true)]
        [Theory]
        public async Task UnlockTestAsync(string db, string password, bool hasKey)
        {
            var kdbx = TestAssets.GetFile(db);
            var unlocker = new KdbxDatabaseCache(Substitute.For<ILogger>(), Substitute.For<IDatabaseFileAccess>());

            var key = hasKey ? TestAssets.GetFile(Path.ChangeExtension(db, "key")) : null;
            var credentials = new KeePassCredentials(key, password);

            var result = await unlocker.UnlockAsync(kdbx, credentials);

            Assert.NotNull(result);
            Assert.Equal(result.Id, kdbx.IdFromPath());
        }

        [InlineData("password-aes_rijndael_256.kdbx", "123456", false)]
        [Theory]
        public async Task InvalidUnlockTestAsync(string db, string password, bool hasKey)
        {
            var kdbx = TestAssets.GetFile(db);
            var unlocker = new KdbxDatabaseCache(Substitute.For<ILogger>(), Substitute.For<IDatabaseFileAccess>());

            var key = hasKey ? TestAssets.GetFile(Path.ChangeExtension(db, "key")) : null;
            var credentials = new KeePassCredentials(key, password);

            await Assert.ThrowsAsync<InvalidCredentialsException>(async () => await unlocker.UnlockAsync(kdbx, credentials));
        }
    }
}
