using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Popups;

namespace KeePass.Win.Services
{
    public class MessageBoxDialogs : IMessageDialogFactory
    {
        public async Task<bool> CheckToDeleteAsync(string type, string name)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            var tcs = new TaskCompletionSource<bool>();

            await dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
            {
                var dialog = new MessageDialog($"Are you sure you want to remove the {type} '{name}'", $"Remove {type}?");
                dialog.Commands.Add(new UICommand { Label = "No", Id = 0 });
                dialog.Commands.Add(new UICommand { Label = "Yes", Id = 1 });

                var result = await dialog.ShowAsync();

                tcs.SetResult((int)result.Id == 1);
            }).AsTask().ConfigureAwait(false);

            return await tcs.Task.ConfigureAwait(false);
        }

        public Task DatabaseAlreadyExistsAsync() => ShowDialogAsync(LocalizedStrings.MenuItemOpenSameFileContent, LocalizedStrings.MenuItemOpenSameFileTitle);

        public Task DatabaseSavedAsync() => ShowDialogAsync("Database saved", string.Empty);

        public Task ErrorSavingDatabaseAsync(Exception e) => ShowDialogAsync(e.Message, "Error saving database");

        public Task InvalidCredentialsAsync() => ShowDialogAsync(LocalizedStrings.InvalidCredentials, LocalizedStrings.MenuItemOpenError);

        public Task UnlockErrorAsync(string message) => ShowDialogAsync(message, LocalizedStrings.MenuItemOpenError);

        private Task ShowDialogAsync(string message, string title)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            return dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
            {
                var dialog = new MessageDialog(message, title);

                await dialog.ShowAsync();
            }).AsTask();
        }
    }
}
