using KeePass.Models;
using System;
using System.Xml.Linq;

namespace KeePass.Services.Cache
{
    public interface ICacheService
    {
        /// <summary>
        /// Gets the cached database.
        /// </summary>
        IKeePassDatabase Database { get; }

        /// <summary>
        /// Gets the root group.
        /// </summary>
        IKeePassGroup Root { get; }

        /// <summary>
        /// Stores the specified database in cache.
        /// </summary>
        /// <param name="database">The dataabase to be cached.</param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="database"/> cannot be <c>null</c>.
        /// </exception>
        void Cache(IKeePassDatabase database);

        /// <summary>
        /// Clears the cache.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the Entry element with the specified UUID.
        /// </summary>
        /// <param name="uuid">The entry's UUID.</param>
        /// <returns>The specified entry, or <c>null</c> if not found.</returns>
        IKeePassEntry GetEntry(KeePassId uuid);

        /// <summary>
        /// Gets the Group element with the specified UUID.
        /// </summary>
        /// <param name="uuid">The group's UUID.</param>
        /// <returns>The specified group, or <c>null</c> if not found.</returns>
        IKeePassGroup GetGroup(KeePassId uuid);
    }
}