using System;

namespace KeePass
{
    public class InvalidCredentialsException : DatabaseUnlockException
    {
        public InvalidCredentialsException(Exception inner)
            : base(string.Empty, inner)
        {
        }
    }
}
