using System;
using Windows.Storage.Streams;
using KeePass.IO.Crypto;

namespace KeePass.IO.Models
{
    /// <summary>
    /// Headers of a KeePass 2.x database file.
    /// </summary>
    public class FileHeaders
    {
        /// <summary>
        /// Gets or sets the encryption IV.
        /// </summary>
        public IBuffer EncryptionIV { get; set; }

        /// <summary>
        /// Gets or sets the header hash.
        /// </summary>
        public IBuffer Hash { get; set; }

        /// <summary>
        /// Gets or sets the master seed.
        /// </summary>
        public IBuffer MasterSeed { get; set; }

        /// <summary>
        /// Gets or sets the protected stream key.
        /// </summary>
        public IBuffer ProtectedStreamKey { get; set; }

        /// <summary>
        /// Gets or sets the algorithm for the random bytes generator.
        /// </summary>
        public CrsAlgorithm RandomAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets the database schema.
        /// Only major and minor version is used.
        /// </summary>
        public Version Schema { get; set; }

        /// <summary>
        /// Gets or sets the first bytes of the decrypted database content.
        /// </summary>
        public IBuffer StartBytes { get; set; }

        /// <summary>
        /// Gets or sets the number of password transformation rounds.
        /// </summary>
        public ulong TransformRounds { get; set; }

        /// <summary>
        /// Gets or sets the password transformation seed.
        /// </summary>
        public IBuffer TransformSeed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the database content is compressed using GZip.
        /// </summary>
        /// <value>
        ///   <c>true</c> if GZip compressed; otherwise, <c>false</c>.
        /// </value>
        public bool UseGZip { get; set; }
    }
}