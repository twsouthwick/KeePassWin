using Autofac;
using KeePass.IO.Database;

namespace KeePassWin
{
    internal class FileServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DatabaseCache>()
                .SingleInstance();

            builder.RegisterType<DatabaseTracker>()
                .SingleInstance();
        }
    }
}
