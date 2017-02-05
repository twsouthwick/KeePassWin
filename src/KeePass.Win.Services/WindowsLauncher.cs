using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.System;

namespace KeePass.Win.Services
{
    public class WindowsLauncher : ILauncher
    {
        public async Task<bool> LaunchUriAsync(Uri uri)
        {
            if (!(await Launcher.LaunchUriAsync(uri).AsTask().ConfigureAwait(false)))
            {
                Debug.WriteLine($"Could not launch {uri}");
                return false;
            }

            return true;
        }
    }
}
