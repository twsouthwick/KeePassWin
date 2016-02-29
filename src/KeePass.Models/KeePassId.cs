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
        private static readonly KeePassId s_empty = new KeePassId(string.Empty);

        private readonly string _id;

        public KeePassId(string id)
        {
            _id = id;
        }

        public string Id => _id;

        public bool IsEmpty => string.IsNullOrEmpty(_id);

        public override bool Equals(object obj)
        {
            var other = obj as KeePassId;

            if (other == null)
            {
                return false;
            }

            return string.Equals(other._id, _id, StringComparison.Ordinal);
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
            return string.IsNullOrEmpty(id) ? s_empty : new KeePassId(id);
        }

        public static explicit operator string(KeePassId id)
        {
            return id.Id;
        }

        public static implicit operator KeePassId(int id)
        {
            return new KeePassId(id.ToString());
        }

        public static KeePassId Empty => s_empty;

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
