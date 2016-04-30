using System.IO;
using System.Threading.Tasks;

namespace KeePass
{
    public interface IFile
    {
        string Name { get; }

        string Path { get; }

        Task<Stream> OpenReadAsync();
    }
}
