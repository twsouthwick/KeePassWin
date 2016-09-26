using KeePassLib;
using KeePassLib.Keys;
using System.Threading.Tasks;
using System;
using System.IO;

namespace KeePass
{
    public struct KdbxBuilder
    {
        private readonly IFile _key;
        private readonly string _password;

        private KdbxBuilder(IFile kdbx, IFile key, string password)
        {
            Kdbx = kdbx;
            _key = key;
            _password = password;
        }

        public IFile Kdbx { get; }

        public static KdbxBuilder Create(IFile kdbx)
        {
            return new KdbxBuilder(kdbx, null, null);
        }

        public KdbxBuilder AddKey(IFile key)
        {
            if (key == null)
            {
                return this;
            }

            return new KdbxBuilder(Kdbx, key, _password);
        }

        public KdbxBuilder AddPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return this;
            }

            return new KdbxBuilder(Kdbx, _key, password);
        }

        public async Task<PwDatabase> CreateDatabaseAsync()
        {
            var compositeKey = new CompositeKey();

            if (_password != null)
            {
                compositeKey.AddUserKey(new KcpPassword(_password));
            }

            if (_key != null)
            {
                compositeKey.AddUserKey(new KcpKeyFile(await _key.ReadFileBytesAsync()));
            }

            return new PwDatabase
            {
                MasterKey = compositeKey
            };
        }
    }
}
