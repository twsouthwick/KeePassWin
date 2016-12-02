using System.Threading.Tasks;

namespace KeePass
{
    public interface IMailClient<T>
    {
        Task SendAsync(T obj);
    }
}
