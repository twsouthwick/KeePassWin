using KeePass.Models;
using System;
using System.Linq;
using System.Xml.Linq;

namespace KeePass.Services.Cache
{
    public sealed class CacheService : ICacheService
    {
        private ILookup<KeePassId, IKeePassGroup> _groups;
        private ILookup<KeePassId, IKeePassEntry> _entries;

        /// <summary>
        /// Gets the cached database.
        /// </summary>
        public IKeePassDatabase Database { get; private set; }

        /// <summary>
        /// Stores the specified database in cache.
        /// </summary>
        /// <param name="database">The dataabase to be cached.</param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="database"/> cannot be <c>null</c>.
        /// </exception>
        public void Cache(IKeePassDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException("database");

            Database = database;

            Root = Database.Groups.FirstOrDefault();

            _groups = Database.Groups.ToLookup(x => x.Id);
            _entries = Database.EnumerateAllEntries()
                .ToLookup(x => x.Id);
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void Clear()
        {
            Database = null;
            _groups = null;
            _entries = null;
        }

        /// <summary>
        /// Gets the Group element with the specified UUID.
        /// </summary>
        /// <param name="uuid">The group's UUID.</param>
        /// <returns>The specified group, or <c>null</c> if not found.</returns>
        public IKeePassGroup GetGroup(KeePassId uuid)
        {
            return _groups != null ? _groups[uuid].FirstOrDefault() : null;
        }

        public IKeePassGroup Root { get; set; }


        public IKeePassEntry GetEntry(KeePassId uuid)
        {
            return _entries == null ? null : _entries[uuid].FirstOrDefault();
        }
    }
}