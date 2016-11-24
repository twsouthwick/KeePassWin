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
                .As<IDatabaseUnlocker>()
                .SingleInstance();

            builder.RegisterType<DialogDatabaseUnlocker>()
                .Named<IDatabaseUnlockerDialog>(nameof(DialogDatabaseUnlocker))
                .SingleInstance();

            builder.RegisterDecorator<IDatabaseUnlockerDialog>((c, inner) => new CachedDatabseUnlocker(inner), fromKey: nameof(DialogDatabaseUnlocker))
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

            builder.RegisterType<AppShellNavPane>()
                .As<INavigationPane>()
                .SingleInstance();
        }

        private class AppShellNavPane : INavigationPane
        {
            private readonly Func<AppShell> _shell;

            /// <summary>
            /// Abstracts navigation pane closing
            /// </summary>
            /// <param name="shell">Ask for a Func to delay generation of AppShell, otherwise a StackOverflow will occur</param>
            public AppShellNavPane(Func<AppShell> shell)
            {
                _shell = shell;
            }

            public void Dismiss() => _shell().Dismiss();

            public void Open() => _shell().Open();
        }
    }
}
