using Autofac.Builder;
using KeePass.Win.AppModel;
using Windows.UI.Xaml;

namespace KeePass.Win
{
    internal static class AutofacExtensions
    {
        /// <summary>
        /// Registers special types that can hook into the UWP application model via app-specific abstractions
        /// </summary>
        /// <typeparam name="TLimit"></typeparam>
        /// <typeparam name="TActivatorData"></typeparam>
        /// <typeparam name="TRegistrationStyle"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> RegisterAppModel<TLimit, TActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder)
        {
            return builder.OnActivated(c =>
            {
                var backgroundAware = c.Instance as IBackgroundEnteredAware;
                if (backgroundAware != null)
                {
                    Application.Current.EnteredBackground += async (s, e) =>
                    {
                        var deferral = e.GetDeferral();
                        await backgroundAware.BackgroundEnteredAsync();
                        deferral.Complete();
                    };
                }
            });
        }
    }
}
