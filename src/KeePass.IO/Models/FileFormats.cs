using System;

namespace KeePass.IO.Models
{
    public enum FileFormats
    {
        /// <summary>
        /// Database file is not a recognized format.
        /// </summary>
        NotSupported,

        /// <summary>
        /// Database file is supported.
        /// </summary>
        Supported,

        /// <summary>
        /// Database file is of a support format, but may contain minor changes.
        /// </summary>
        PartialSupported,

        /// <summary>
        /// Database file is a KeePass 1.x database file, which is not supported.
        /// </summary>
        KeePass1x,

        /// <summary>
        /// Database file a known old format that is not supported.
        /// </summary>
        OldVersion,

        /// <summary>
        /// Database file is newer format than is not supported.
        /// </summary>
        NewVersion,
    }
}