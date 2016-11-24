using System.Threading.Tasks;

namespace KeePass
{
    public interface IDatabaseUnlockerDialog
    {
        Task<IKeePassDatabase> UnlockAsync(IFile file);
    }
}
