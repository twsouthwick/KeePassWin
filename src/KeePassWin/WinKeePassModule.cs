using Autofac;
using KeePass.IO;
using KeePass.IO.Database;
using KeePass.Models;

namespace KeePassWin
{
    internal class WinKeePassModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DatabaseCache>()
                .SingleInstance();

            builder.RegisterType<DatabaseTracker>()
                .SingleInstance();

            builder.RegisterType<DialogDatabaseUnlocker>()
                .As<IDatabaseUnlocker>()
                .SingleInstance();
        }
    }
}
