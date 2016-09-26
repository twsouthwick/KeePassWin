using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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

        public static string IdFromPath(this IFile file)
        {
            using (var md5 = MD5.Create())
            {
                var bytes = Encoding.Unicode.GetBytes(file.Path.ToLowerInvariant());
                var hash = md5.ComputeHash(bytes);

                return BitConverter.ToString(hash);
            }
        }
    }
}
