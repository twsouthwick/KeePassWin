using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace KeePass
{
    public class EncryptedDatabaseUnlocker : IDatabaseUnlocker
    {
        public virtual Task<IKeePassDatabase> UnlockAsync(IFile file)
        {
            return UnlockAsync(file, null, null);
        }

        public async Task<IKeePassDatabase> UnlockAsync(IFile file, IFile keyfile, string password)
        {
            try
            {
                return await KeePassLibWrapper.Load(file, keyfile, password);
            }
            catch (DatabaseUnlockException)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);

                throw new DatabaseUnlockException($"Unknown error: {e.Message}", e);
            }
        }
    }
}
