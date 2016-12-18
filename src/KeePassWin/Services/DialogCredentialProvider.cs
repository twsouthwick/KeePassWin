using KeePass.Win.Views;
using System;
using System.Threading.Tasks;

namespace KeePass.Win.Services
{
    public class DialogCredentialProvider : ICredentialProvider
    {
        private readonly Func<IFile, PasswordDialog> _dialogFactory;
        private readonly IDatabaseFileAccess _tracker;

        public DialogCredentialProvider(Func<IFile, PasswordDialog> dialogFactory, IDatabaseFileAccess tracker)
        {
            _dialogFactory = dialogFactory;
            _tracker = tracker;
        }

        public async Task<KeePassCredentials> GetCredentialsAsync(IFile file)
        {
            var dialog = _dialogFactory(file);
            var model = await dialog.GetModelAsync();

            if (model == null)
            {
                return default(KeePassCredentials);
            }

            return new KeePassCredentials(model.KeyFile, model.Password);
        }
    }
}
