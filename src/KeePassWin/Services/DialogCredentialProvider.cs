using KeePass.Win.Views;
using System;
using System.Threading.Tasks;

namespace KeePass.Win.Services
{
    public class DialogCredentialProvider : ICredentialProvider
    {
        private readonly Func<IFile, PasswordDialog> _dialogFactory;

        public DialogCredentialProvider(Func<IFile, PasswordDialog> dialogFactory)
        {
            _dialogFactory = dialogFactory;
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
