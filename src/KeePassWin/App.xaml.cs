using Autofac;
using KeePass.Win.ViewModels;
using KeePass.Win.Views;
using Prism.Autofac.Windows;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KeePass.Win
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : PrismAutofacApplication
    {
        private static readonly Dictionary<Type, Type> s_viewModelMap = new Dictionary<Type, Type>
        {
            { typeof(DatabasePage), typeof(DatabasePageViewModel) },
            { typeof(SettingsPage), typeof(SettingsPageViewModel) },
        };

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
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

        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            base.ConfigureContainer(builder);

            builder.RegisterType<AppShell>()
                .SingleInstance();

            builder.RegisterModule<WinKeePassModule>();
        }

        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(view =>
            {
                Type viewModel;
                if (s_viewModelMap.TryGetValue(view, out viewModel))
                {
                    return viewModel;
                }

                return null;
            });
        }
    }
}
