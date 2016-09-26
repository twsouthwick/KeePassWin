using System.Threading.Tasks;

namespace KeePass
{
    public interface IDatabaseUnlocker
    {
        Task<IKeePassDatabase> UnlockAsync(KdbxBuilder builder);
    }
}
