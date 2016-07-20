using System.Collections.Generic;
using System.Threading.Tasks;

namespace KeePass
{

    public interface IKeePassIdGenerator
    {
        KeePassId FromPath(string path);
    }

    public interface IDatabaseTracker
    {
        Task<bool> AddDatabaseAsync(IFile dbFile);
        Task AddKeyFileAsync(IFile dbFile, IFile keyFile);
        Task<IFile> GetDatabaseAsync(KeePassId id);
        Task<IEnumerable<IFile>> GetDatabasesAsync();
        Task<IFile> GetKeyFileAsync(IFile dbFile);
        Task RemoveDatabaseAsync(IFile dbFile);
    }
}