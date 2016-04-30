using System;

namespace KeePass
{
    public class DatabaseUnlockException : Exception
    {
        public DatabaseUnlockException(string message)
            : base(message)
        { }

        public DatabaseUnlockException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
