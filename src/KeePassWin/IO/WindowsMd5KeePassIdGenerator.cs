using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace KeePass
{
    public class WindowsMd5KeePassIdGenerator : IKeePassIdGenerator
    {
        /// <summary>
        /// Generate a MD5 hash of the file name to ensure the tokens are always the same
        /// </summary>
        /// <param name = "path"></param>
        /// <returns></returns>
        public KeePassId FromPath(string path)
        {
            var algorithm = HashAlgorithmProvider
                .OpenAlgorithm(HashAlgorithmNames.Md5);
            var buffer = CryptographicBuffer
                .ConvertStringToBinary(path.ToLowerInvariant(), BinaryStringEncoding.Utf16BE);
            var hash = algorithm.CreateHash();

            hash.Append(buffer);

            return CryptographicBuffer.EncodeToHexString(hash.GetValueAndReset());
        }
    }
}