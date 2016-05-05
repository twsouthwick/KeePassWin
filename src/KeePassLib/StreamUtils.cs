using System;
using System.IO;
using System.Threading.Tasks;

namespace KeePass
{
    internal static class StreamUtils
    {
        public static Task<byte[]> ReadAsync(this Stream stream, int count)
        {
            var bytes = new byte[count];

            return stream.ReadAsync(bytes, 0, count).ContinueWith(r =>
            {
                if (r.Result != count)
                {
                    return Array.Empty<byte>();
                }

                return bytes;
            });
        }
    }
}