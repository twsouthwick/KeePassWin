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
                .Named<IDatabaseCache>(nameof(DatabaseCache))
                .SingleInstance();

            builder.RegisterType<FileDatabaseTracker>()
                .As<IDatabaseFileAccess>()
                .SingleInstance();

            builder.RegisterType<DialogCredentialProvider>()
                .As<ICredentialProvider>()
                .SingleInstance();

            builder.RegisterDecorator<IDatabaseCache>((c, inner) => new SimpleDatabaseCache(inner), fromKey: nameof(DatabaseCache))
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
