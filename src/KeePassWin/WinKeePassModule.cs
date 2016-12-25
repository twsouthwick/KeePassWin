using Autofac;
using KeePass.Win.Controls;
using KeePass.Win.Log;
using KeePass.Win.Mvvm;
using KeePass.Win.Services;
using Serilog;
using System.Threading.Tasks;

namespace KeePass.Win
{
    internal class WinKeePassModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<KdbxDatabaseCache>()
                .Named<IDatabaseCache>(nameof(DatabaseCache))
                .SingleInstance();

            builder.RegisterType<FileDatabaseTracker>()
                .As<IDatabaseFileAccess>()
                .SingleInstance();

            builder.RegisterType<DialogCredentialProvider>()
                .As<ICredentialProvider>()
                .SingleInstance();

            builder.RegisterDecorator<IDatabaseCache>((c, inner) => new SimpleDatabaseCache(inner, c.Resolve<INavigator>()), fromKey: nameof(DatabaseCache))
                .RegisterAppModel()
                .SingleInstance();

            builder.RegisterType<DataPackageClipboard>()
                .As<IClipboard<string>>()
                .As<IClipboard<ILogView>>()
                .As<IMailClient<ILogView>>()
                .SingleInstance();

            builder.RegisterType<PrismNavigationService>()
                .As<INavigator>()
                .SingleInstance();

            builder.RegisterType<WindowsFilePicker>()
                .As<IFilePicker>()
                .SingleInstance();

            builder.RegisterType<StringBuilderSink>()
                .AsSelf()
                .As<ILogView>()
                .SingleInstance();

            builder.RegisterType<WinKeePassSettings>()
                .As<KeePassSettings>()
                .OnActivated(s => s.Instance.Load())
                .SingleInstance();

            builder.Register(CreateLogger)
                .As<ILogger>()
                .SingleInstance();

            builder.RegisterType<WindowsKeyboardShortcuts>()
                .As<KeyboardShortcuts>()
                .SingleInstance();

            builder.RegisterType<DialogNameProvider>()
                .As<INameProvider>()
                .SingleInstance();
        }

        private ILogger CreateLogger(IComponentContext arg)
        {
            var log = new LoggerConfiguration()
#if DEBUG
                .WriteTo.Trace()
#endif
                .WriteTo.Sink(arg.Resolve<StringBuilderSink>())
                .CreateLogger();

            return new SerilogLogger(log);
        }

        private class DialogNameProvider : INameProvider
        {
            public Task<string> GetNameAsync(string initial = null)
            {
                var dialog = new NameBoxDialog();

                return dialog.GetNameAsync(initial);
            }
        }
    }
}
