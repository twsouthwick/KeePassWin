using System.Collections.Generic;

namespace KeePass
{
    public static class KeePassExtensions
    {
        public static IKeePassGroup GetGroup(this IKeePassDatabase db, KeePassId id)
        {
            foreach (var entry in db.EnumerateAllGroups())
            {
                if (entry.Id.Equals(id))
                {
                    return entry;
                }
            }

            return null;
        }

        public static IEnumerable<IKeePassEntry> EnumerateAllEntries(this IKeePassDatabase db)
        {
            return db.Root.EnumerateAllEntries();
        }

        public static IEnumerable<IKeePassGroup> EnumerateAllGroups(this IKeePassDatabase db)
        {
            return db.Root.EnumerateAllGroups();
        }

        public static IEnumerable<IKeePassGroup> EnumerateAllGroups(this IKeePassGroup group)
        {
            foreach (var subgroup in group.Groups)
            {
                yield return subgroup;
                foreach (var entry in subgroup.EnumerateAllGroups())
                {
                    yield return entry;
                }
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

        public static IEnumerable<IKeePassGroup> EnumerateParents(this IKeePassGroup group)
        {
            if (group == null)
            {
                yield break;
            }

            while (group != null && group.Parent != null)
            {
                yield return group.Parent;

                group = group.Parent;
            }
        }
    }
}
