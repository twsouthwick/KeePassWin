using Autofac;
using KeePass.Win.Mvvm;
using KeePass.Win.Services;
using System;

namespace KeePass.Win
{
    internal class WinKeePassModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DatabaseCache>()
                .SingleInstance();

            builder.RegisterType<FileDatabaseTracker>()
                .As<IDatabaseTracker>()
                .SingleInstance();

            builder.RegisterType<KdbxUnlocker>()
                .As<IKdbxUnlocker>()
                .SingleInstance();

            builder.RegisterType<DialogDatabaseUnlocker>()
                .Named<IDatabaseCache>(nameof(DialogDatabaseUnlocker))
                .SingleInstance();

            builder.RegisterDecorator<IDatabaseCache>((c, inner) => new CachedDatabseUnlocker(inner), fromKey: nameof(DialogDatabaseUnlocker))
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
    }
}
