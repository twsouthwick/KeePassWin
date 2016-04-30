using System.Collections.Generic;
using System.Threading.Tasks;
using KeePass.Models;

namespace KeePass.IO.Database
{
    public interface IDatabaseTracker
    {
        Task<bool> AddDatabaseAsync(IFile dbFile);
        Task AddKeyFileAsync(IFile dbFile, IFile keyFile);
        Task<IFile> GetDatabaseAsync(KeePassId id);
        Task<IEnumerable<IFile>> GetDatabasesAsync();
        Task<IFile> GetKeyFileAsync(IFile dbFile);
    }
}