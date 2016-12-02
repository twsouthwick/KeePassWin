using Autofac;
using KeePass.Win.Log;
using KeePass.Win.Mvvm;
using KeePass.Win.Services;
using Serilog;

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

            builder.Register(CreateLogger)
                .As<ILogger>()
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
    }
}
