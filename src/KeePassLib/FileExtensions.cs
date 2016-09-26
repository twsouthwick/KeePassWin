using System.IO;
using System.Threading.Tasks;

namespace KeePass
{

    public static class FileExtensions
    {
        public static async Task<byte[]> ReadFileBytesAsync(this IFile file)
        {
            using (var fs = await file.OpenReadAsync())
            using (var ms = new MemoryStream())
            {
                await fs.CopyToAsync(ms);

                return ms.ToArray();
            }
        }
    }
}
