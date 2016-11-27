using System.Threading.Tasks;

namespace KeePass
{
    public interface IKdbxUnlocker
    {
        Task<IKeePassDatabase> UnlockAsync(KdbxBuilder builder);
    }
}
