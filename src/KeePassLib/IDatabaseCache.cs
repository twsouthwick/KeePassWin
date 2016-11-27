using System.Collections.Generic;
using System.Threading.Tasks;

namespace KeePass
{
    public interface IDatabaseCache
    {
        Task<IKeePassDatabase> UnlockAsync(KeePassId id, ICredentialProvider credentialProvider);

        Task<IFile> AddDatabaseAsync();

        Task<IEnumerable<IFile>> GetDatabaseFilesAsync();

        Task RemoveDatabaseAsync(IFile dbFile);
    }
}
