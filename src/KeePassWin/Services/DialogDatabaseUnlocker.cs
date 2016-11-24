using KeePass.Win.Views;
using System;
using System.Threading.Tasks;

namespace KeePass.Win.Services
{
    public class DialogDatabaseUnlocker : IDatabaseUnlockerDialog
    {
        private readonly Func<IFile, PasswordDialog> _dialogFactory;
        private readonly IDatabaseUnlocker _kdbxUnlocker;

        public DialogDatabaseUnlocker(Func<IFile, PasswordDialog> dialogFactory, IDatabaseUnlocker kdbxUnlocker)
        {
            _kdbxUnlocker = kdbxUnlocker;
            _dialogFactory = dialogFactory;
        }

        public async Task<IKeePassDatabase> UnlockAsync(IFile file)
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
