using System.Threading.Tasks;

namespace KeePass.Models
{
    public interface IDatabaseUnlocker
    {
        Task<IKeePassDatabase> UnlockAsync(IFile file);
    }
}
