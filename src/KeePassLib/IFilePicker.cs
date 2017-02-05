using System.Threading.Tasks;

namespace KeePass
{
    public interface IFilePicker
    {
        Task<IFile> GetDatabaseAsync();

        Task<IFile> GetKeyFileAsync();
    }
}
