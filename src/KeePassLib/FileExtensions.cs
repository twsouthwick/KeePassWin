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

        public static KeePassId IdFromPath(this IFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            using (var md5 = MD5.Create())
            {
                var bytes = Encoding.Unicode.GetBytes(file.Path.ToLowerInvariant());
                var hash = md5.ComputeHash(bytes);

                // Convert manually to Guid since we want to read it as Big Endian
                var buffer = new byte[]
                {
                    hash[3],
                    hash[2],
                    hash[1],
                    hash[0],
                    hash[5],
                    hash[4],
                    hash[7],
                    hash[6],
                    hash[8],
                    hash[9],
                    hash[10],
                    hash[11],
                    hash[12],
                    hash[13],
                    hash[14],
                    hash[15]
                };

                return new KeePassId(new Guid(buffer));
            }
        }
    }
}
