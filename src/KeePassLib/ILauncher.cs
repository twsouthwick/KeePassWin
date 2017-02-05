using System;
using System.Threading.Tasks;

namespace KeePass
{
    public interface ILauncher
    {
        Task<bool> LaunchUriAsync(Uri uri);
    }
}
