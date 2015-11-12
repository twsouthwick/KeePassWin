using System.Threading.Tasks;
using Windows.Storage;

namespace KeePass.Models
{
    public interface IDatabaseUnlocker
    {
        Task<IKeePassDatabase> UnlockAsync(IStorageFile file);
    }
}
