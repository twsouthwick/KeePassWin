using KeePass.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KeePass.IO
{
    internal class CachedDatabseUnlocker : IDatabaseUnlocker
    {
        private readonly IDictionary<string, IKeePassDatabase> _cache = new Dictionary<string, IKeePassDatabase>(StringComparer.OrdinalIgnoreCase);
        private readonly IDatabaseUnlocker _unlocker;

        public CachedDatabseUnlocker(IDatabaseUnlocker unlocker)
        {
            _unlocker = unlocker;
        }

        public async Task<IKeePassDatabase> UnlockAsync(IFile file)
        {
            IKeePassDatabase db;
            if (_cache.TryGetValue(file.Path, out db))
            {
                return db;
            }

            var unlocked = await _unlocker.UnlockAsync(file);

            _cache.Add(file.Path, unlocked);

            return unlocked;
        }
    }
}
