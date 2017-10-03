using Autofac;
using Prism.Autofac.Windows;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KeePass.Win
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : PrismAutofacApplication
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class. This is the first line
        /// of authored code executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        protected override UIElement CreateShell(Frame rootFrame)
        {
            var shell = Container.Resolve<AppShell>();
            shell.SetContentFrame(rootFrame);
            return shell;
        }

        protected override Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
            Container.Resolve<INavigator>().GoToMain();

            return Task.CompletedTask;
        }

        protected override async Task OnActivateApplicationAsync(IActivatedEventArgs args)
        {
            Container.Resolve<ILogger>().Info("Application was activated {ActivationArg}", args);

            if (args.Kind == ActivationKind.File && args is FileActivatedEventArgs fileArgs)
            {
                var cache = Container.Resolve<IDatabaseCache>();

                foreach (var file in fileArgs.Files)
                {
                    await cache.AddDatabaseAsync(new FilePickerSingleFile(file.AsFile()), true);
                }
            }
        }

        private class FilePickerSingleFile : IFilePicker
        {
            private readonly IFile _file;

            public FilePickerSingleFile(IFile file)
            {
                _file = file;
            }

            public Task<IFile> GetDatabaseAsync() => Task.FromResult(_file);

            public Task<IFile> GetKeyFileAsync()
            {
                throw new System.NotImplementedException();
            }
        }

        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            base.ConfigureContainer(builder);

            builder.RegisterType<AppShell>()
                .SingleInstance();

            builder.RegisterModule<WinKeePassModule>();
        }
    }
}
