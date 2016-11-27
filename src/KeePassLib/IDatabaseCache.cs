using System.Threading.Tasks;

namespace KeePass
{
    public interface IDatabaseCache
    {
        Task<IKeePassDatabase> UnlockAsync(KeePassId id);
    }
}
