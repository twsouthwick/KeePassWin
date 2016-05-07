using KeePass.Crypto;
using KeePassWin.Views;
using System;
using System.Threading.Tasks;

namespace KeePass
{
    public class DialogDatabaseUnlocker : EncryptedDatabaseUnlocker, IDatabaseUnlocker
    {
        private readonly Func<IFile, PasswordDialog> _dialogFactory;

        public DialogDatabaseUnlocker(Func<IFile, PasswordDialog> dialogFactory, FileFormat fileFormat, ICryptoProvider hashProvider)
            : base(fileFormat, hashProvider)
        {
            _dialogFactory = dialogFactory;
        }

        public override async Task<IKeePassDatabase> UnlockAsync(IFile file)
        {
            var dialog = _dialogFactory(file);

            await dialog.ShowAsync();

            if (dialog.Result == PasswordDialog.ResultState.Cancel || dialog.Result == PasswordDialog.ResultState.None)
            {
                return null;
            }

            var model = dialog.Model;

            return await UnlockAsync(file, model.KeyFile, dialog.Model.Password);
        }
    }
}
