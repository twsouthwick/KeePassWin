using System.Threading.Tasks;

namespace KeePass
{
    public interface ICredentialProvider
    {
        Task<KeePassCredentials> GetCredentialsAsync(IFile file);
    }
}
