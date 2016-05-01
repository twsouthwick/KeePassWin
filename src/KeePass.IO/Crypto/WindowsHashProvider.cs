using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography.Core;

namespace KeePass.Crypto
{
    public class WindowsHashProvider : IHashProvider
    {
        public byte[] GetSha256(byte[] input)
        {
            return HashAlgorithmProvider
               .OpenAlgorithm(HashAlgorithmNames.Sha256)
               .HashData(input.AsBuffer())
               .ToArray();
        }
    }
}
