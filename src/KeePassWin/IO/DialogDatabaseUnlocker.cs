using KeePass.IO.Database;
using KeePass.Models;
using KeePassWin.Views;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace KeePass.IO
{
    public class DialogDatabaseUnlocker : EncryptedDatabaseUnlocker, IDatabaseUnlocker
    {
        private readonly Func<IStorageFile, PasswordDialog> _dialogFactory;

        public DialogDatabaseUnlocker(Func<IStorageFile, PasswordDialog> dialogFactory)
        {
            _dialogFactory = dialogFactory;
        }

        public override async Task<IKeePassDatabase> UnlockAsync(IStorageFile file)
        {
            var dialog = _dialogFactory(file);

            await dialog.ShowAsync();

            if (dialog.Result == PasswordDialog.ResultState.Cancel || dialog.Result == PasswordDialog.ResultState.None)
            {
                return null;
            }

            var model = dialog.Model;

            return await UnlockAsync(file, model.KeyFile as IStorageFile, dialog.Model.Password);
        }
    }
}
