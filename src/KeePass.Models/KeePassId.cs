using System;
using System.Diagnostics;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;

namespace KeePass.Models
{
    [DebuggerDisplay("KeePass ID: {Id}")]
    public class KeePassId
    {
        private readonly string _id;

        public KeePassId(string id)
        {
            _id = id;
        }

        public string Id => _id;

        public override bool Equals(object obj)
        {
            return string.Equals(obj as string, _id, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override string ToString()
        {
            return _id;
        }

        public static implicit operator KeePassId(string id)
        {
            return new KeePassId(id);
        }

        public static explicit operator string (KeePassId id)
        {
            return id.Id;
        }

        /// <summary>
        /// Generate a MD5 hash of the file name to ensure the tokens are always the same
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static KeePassId FromPath(IStorageFile file)
        {
            var algorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var buffer = CryptographicBuffer.ConvertStringToBinary(file.Path.ToLowerInvariant(), BinaryStringEncoding.Utf16BE);
            var hash = algorithm.CreateHash();

            hash.Append(buffer);

            var result = hash.GetValueAndReset();

            return CryptographicBuffer.EncodeToHexString(result);
        }
    }
}
