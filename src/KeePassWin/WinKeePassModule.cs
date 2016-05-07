using Autofac;
using KeePass;
using KeePass.Crypto;
using KeePass.IO;
using KeePassWin.Mvvm;
using System;

namespace KeePassWin
{
    internal class WinKeePassModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DatabaseCache>()
                .SingleInstance();

            BuildCryptoProviders(builder);

            builder.RegisterType<SqliteDatabaseTracker>()
                .As<IDatabaseTracker>()
                .SingleInstance();

            builder.RegisterType<DialogDatabaseUnlocker>()
                .Named<IDatabaseUnlocker>(nameof(DialogDatabaseUnlocker))
                .SingleInstance();

            builder.RegisterDecorator<IDatabaseUnlocker>((c, inner) => new CachedDatabseUnlocker(inner), fromKey: nameof(DialogDatabaseUnlocker))
                .SingleInstance();

            builder.RegisterType<TimedClipboard>()
                .WithParameter(TypedParameter.From(TimeSpan.FromSeconds(10)))
                .As<IClipboard>()
                .SingleInstance();

            builder.RegisterType<PrismNavigationService>()
                .As<INavigator>()
                .SingleInstance();

            builder.RegisterType<WindowsFilePicker>()
                .As<IFilePicker>()
                .SingleInstance();
        }

        private void BuildCryptoProviders(ContainerBuilder builder)
        {
            builder.RegisterType<FileFormat>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<WindowsHashProvider>()
                .As<ICryptoProvider>()
                .SingleInstance();

        }
    }
}
