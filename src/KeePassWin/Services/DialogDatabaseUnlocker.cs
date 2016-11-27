using KeePass.Win.Views;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace KeePass.Win.Services
{
    public class DialogDatabaseUnlocker : IDatabaseCache
    {
        private readonly Func<IFile, PasswordDialog> _dialogFactory;
        private readonly IKdbxUnlocker _kdbxUnlocker;
        private readonly IDatabaseTracker _tracker;

        public DialogDatabaseUnlocker(Func<IFile, PasswordDialog> dialogFactory, IKdbxUnlocker kdbxUnlocker, IDatabaseTracker tracker)
        {
            _kdbxUnlocker = kdbxUnlocker;
            _dialogFactory = dialogFactory;
            _tracker = tracker;
        }

        public async Task<IKeePassDatabase> UnlockAsync(KeePassId id)
        {
            var dbFile = await _tracker.GetDatabaseAsync(id);

            Debug.Assert(dbFile != null);

            try
            {
                return await UnlockAsync(dbFile);
            }
            catch (DatabaseUnlockException e)
            {
                var message = new MessageDialog(e.Message, "Could not open database");

                await message.ShowAsync();
                return null;
            }
        }

        private async Task<IKeePassDatabase> UnlockAsync(IFile file)
        {
            var dialog = _dialogFactory(file);

            await dialog.ShowAsync();

            if (dialog.Result == PasswordDialog.ResultState.Cancel || dialog.Result == PasswordDialog.ResultState.None)
            {
                return null;
            }

            var model = dialog.Model;

            var builder = KdbxBuilder.Create(file)
                .AddKey(model.KeyFile)
                .AddPassword(dialog.Model.Password);

            return await _kdbxUnlocker.UnlockAsync(builder);
        }
    }
}
