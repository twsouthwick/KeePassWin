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
            var keyBuilder = KdbxBuilder.Create(TestAssets.GetFile(db))
                .AddPassword(password)
                .AddKey(hasKey ? TestAssets.GetFile(Path.GetFileNameWithoutExtension(db)) : null);

            var unlocker = new KdbxUnlocker();
            var result = await unlocker.UnlockAsync(keyBuilder);

            Assert.NotNull(result);
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
        }
    }
}
