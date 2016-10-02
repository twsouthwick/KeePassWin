using KeePassLib.Serialization;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace KeePass
{
    public class KdbxUnlocker : IDatabaseUnlocker
    {
        public async Task<IKeePassDatabase> UnlockAsync(KdbxBuilder builder)
        {
            try
            {
                var db = await builder.CreateDatabaseAsync();
                var kdbx = new KdbxFile(db);

                using (var fs = await builder.Kdbx.OpenReadAsync())
                {
                    await Task.Run(() =>
                    {
                        kdbx.Load(fs, KdbxFormat.Default, null);
                    });

                    return new KdbxDatabase(builder.Kdbx, db, builder.Kdbx.IdFromPath());
                }
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
