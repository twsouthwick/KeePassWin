using System.Threading.Tasks;

namespace KeePass
{
    public interface INameProvider
    {
        Task<string> GetNameAsync(string initial = null);
    }
}
