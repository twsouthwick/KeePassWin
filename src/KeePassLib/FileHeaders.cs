using System;

namespace KeePass
{
    /// <summary>
    /// Headers of a KeePass 2.x database file.
    /// </summary>
    public class FileHeaders
    {
        /// <summary>
        /// Gets or sets the encryption IV.
        /// </summary>
        public byte[] EncryptionIV { get; set; }

        /// <summary>
        /// Gets or sets the header hash.
        /// </summary>
        public byte[] Hash { get; set; }

        /// <summary>
        /// Gets or sets the master seed.
        /// </summary>
        public byte[] MasterSeed { get; set; }

        /// <summary>
        /// Gets or sets the protected stream key.
        /// </summary>
        public byte[] ProtectedStreamKey { get; set; }

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
        public byte[] StartBytes { get; set; }

        /// <summary>
        /// Gets or sets the number of password transformation rounds.
        /// </summary>
        public ulong TransformRounds { get; set; }

        /// <summary>
        /// Gets or sets the password transformation seed.
        /// </summary>
        public byte[] TransformSeed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the database content is compressed using GZip.
        /// </summary>
        /// <value>
        ///   <c>true</c> if GZip compressed; otherwise, <c>false</c>.
        /// </value>
        public bool UseGZip { get; set; }
    }
}