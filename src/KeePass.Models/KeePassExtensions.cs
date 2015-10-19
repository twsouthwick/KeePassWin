using System.Collections.Generic;
using System.Linq;

namespace KeePass.Models
{
    public static class KeePassExtensions
    {
        public static IEnumerable<IKeePassEntry> EnumerateAllEntries(this IKeePassDatabase db)
        {
            foreach (var entry in db.Groups.SelectMany(EnumerateAllEntries))
            {
                yield return entry;
            }
        }

        public static IEnumerable<IKeePassEntry> EnumerateAllEntries(this IKeePassGroup group)
        {
            foreach (var entry in group.Entries)
            {
                yield return entry;
            }

            foreach (var subgroup in group.Groups)
            {
                foreach (var entry in subgroup.EnumerateAllEntries())
                {
                    yield return entry;
                }
            }
        }
    }
}
