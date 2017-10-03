using KeePassLib;
using KeePassLib.Keys;
using KeePassLib.Serialization;
using System.Threading.Tasks;

namespace KeePass
{
    public class KdbxDatabaseCache : DatabaseCache
    {
        public KdbxDatabaseCache(ILogger log, IDatabaseFileAccess databaseTracker)
            : base(log, databaseTracker)
        {
        }

        public override async Task<IKeePassDatabase> UnlockAsync(IFile dbFile, KeePassCredentials credentials)
        {
            Log.Info("Unlocking {Database}", dbFile);

            try
            {
                var compositeKey = new CompositeKey();

                if (credentials.Password != null)
                {
                    compositeKey.AddUserKey(new KcpPassword(credentials.Password));
                }

                if (credentials.KeyFile != null)
                {
                    compositeKey.AddUserKey(new KcpKeyFile(await credentials.KeyFile.ReadFileBytesAsync()));
                }

                var db = new PwDatabase
                {
                    MasterKey = compositeKey
                };

                var kdbx = new KdbxFile(db);

                using (var fs = await dbFile.OpenReadAsync())
                {
                    await Task.Run(() =>
                    {
                        kdbx.Load(fs, KdbxFormat.Default, null);
                    });

                    return new KdbxDatabase(dbFile, db, dbFile.IdFromPath());
                }
            }
            catch (InvalidCompositeKeyException e)
            {
                throw new InvalidCredentialsException(e);
            }
        }
    }
}
