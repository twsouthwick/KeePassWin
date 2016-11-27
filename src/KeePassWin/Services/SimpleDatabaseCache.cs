using System.Collections.Generic;
using System.Threading.Tasks;

namespace KeePass.Win.Services
{
    internal class SimpleDatabaseCache : IDatabaseCache
    {
        private readonly Dictionary<KeePassId, IKeePassDatabase> _idCache = new Dictionary<KeePassId, IKeePassDatabase>();
        private readonly IDatabaseCache _cache;

        public SimpleDatabaseCache(IDatabaseCache cache)
        {
            _cache = cache;
        }

        public Task<IFile> AddDatabaseAsync() => _cache.AddDatabaseAsync();

        public Task<IEnumerable<IFile>> GetDatabaseFilesAsync() => _cache.GetDatabaseFilesAsync();

        public Task RemoveDatabaseAsync(IFile dbFile) => _cache.RemoveDatabaseAsync(dbFile);

        public async Task<IKeePassDatabase> UnlockAsync(KeePassId id, ICredentialProvider provider)
        {
            IKeePassDatabase db;
            if (_idCache.TryGetValue(id, out db))
            {
                return db;
            }

            var unlocked = await _cache.UnlockAsync(id, provider);

            if (unlocked != null)
            {
                _idCache.Add(id, unlocked);
            }

            return unlocked;
        }
    }
}
