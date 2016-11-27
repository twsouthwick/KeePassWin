using System.Collections.Generic;
using System.Threading.Tasks;

namespace KeePass.Win.Services
{
    internal class CachedDatabseUnlocker : IDatabaseCache
    {
        private readonly Dictionary<KeePassId, IKeePassDatabase> _cache = new Dictionary<KeePassId, IKeePassDatabase>();
        private readonly IDatabaseCache _unlocker;

        public CachedDatabseUnlocker(IDatabaseCache unlocker)
        {
            _unlocker = unlocker;
        }

        public async Task<IKeePassDatabase> UnlockAsync(KeePassId id)
        {
            IKeePassDatabase db;
            if (_cache.TryGetValue(id, out db))
            {
                return db;
            }

            var unlocked = await _unlocker.UnlockAsync(id);

            if (unlocked != null)
            {
                _cache.Add(id, unlocked);
            }

            return unlocked;
        }
    }
}
