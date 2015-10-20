using Autofac;
using Windows.UI.Xaml.Navigation;

namespace KeePassWin.Mvvm
{

    public class Page : Windows.UI.Xaml.Controls.Page
    {

        private static readonly IContainer _container;

        static Page()
        {
            _container = RegisterDependencies();
        }

        // This is set in OnNavigatedTo method
        private ILifetimeScope _scope;

        protected virtual void RegisterItems(ContainerBuilder builder, object parameter)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.Unloaded += PodcastPageUnloaded;

            _scope = _container.BeginLifetimeScope(builder => RegisterItems(builder, e.Parameter));

            _scope.InjectProperties(this);
        }

        private void PodcastPageUnloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Unloaded -= PodcastPageUnloaded;

            _scope?.Dispose();
        }

        private static IContainer RegisterDependencies()
        {
            var builder = new ContainerBuilder();

            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                builder.RegisterModule<DesignerModule>();
            }
            else
            {
                builder.RegisterModule<KeePassModule>();
            }

            return builder.Build();
        }
    }
}
