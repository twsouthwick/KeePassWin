using KeePass;
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
            var unlocker = new EncryptedDatabaseUnlocker();
            var result = await unlocker.UnlockAsync(
                 TestAssets.GetFile(db),
                 hasKey ? TestAssets.GetFile($"{Path.GetFileNameWithoutExtension(db)}.key") : null,
                 password);

            Assert.NotNull(result);
        }
    }
}
