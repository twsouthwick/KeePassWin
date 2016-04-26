using System.Collections.Generic;
using System.Threading.Tasks;
using KeePass.Models;
using Windows.Storage;

namespace KeePass.IO.Database
{
    public interface IDatabaseTracker
    {
        Task<bool> AddDatabaseAsync(IStorageFile dbFile);
        Task AddKeyFileAsync(IStorageFile dbFile, IStorageFile keyFile);
        Task<IStorageFile> GetDatabaseAsync(KeePassId id);
        Task<IEnumerable<IStorageFile>> GetDatabasesAsync();
        Task<IStorageFile> GetKeyFileAsync(IStorageFile dbFile);
    }
}