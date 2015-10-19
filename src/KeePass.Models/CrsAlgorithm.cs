using System;

namespace KeePass.IO.Crypto
{
    public enum CrsAlgorithm
    {
        /// <summary>
        /// A variant of the ARCFour algorithm (RC4 incompatible).
        /// </summary>
        ArcFourVariant = 1,

        /// <summary>
        /// Salsa20 stream cipher algorithm.
        /// </summary>
        Salsa20 = 2,
    }
}