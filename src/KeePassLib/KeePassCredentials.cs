namespace KeePass
{
    public struct KeePassCredentials
    {
        public KeePassCredentials(IFile keyFile, string password)
        {
            KeyFile = keyFile;
            Password = password;
        }

        public IFile KeyFile { get; }

        public string Password { get; }
    }
}
