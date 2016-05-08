using Autofac;
using Autofac.Extras.Moq;
using KeePass;
using KeePass.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace KeePassLib
{
    public class DecryptorTests
    {
        [Fact]
        public async Task JustPassword()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<DotNetHashProvider>().As<ICryptoProvider>().SingleInstance();
            builder.RegisterType<EncryptedDatabaseUnlocker>();
            builder.RegisterType<FileFormat>();
            builder.RegisterType<HashedStream>();
            builder.RegisterType<TestIdGenerator>().As<IKeePassIdGenerator>();

            using (var container = builder.Build())
            {
                var unlocker = container.Resolve<EncryptedDatabaseUnlocker>();

                var db = TestAssets.GetFile("password-aes_rijndael_256.kdbx");
                var password = "12345";

                var result = await unlocker.UnlockAsync(db, null, password);

                Assert.NotNull(result);
            }
        }
    }
}
