using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace KeePass
{
    public static class KeePassExtensions
    {
        public static IKeePassEntry Copy(this IKeePassEntry entry)
        {
            var result = new ReadWriteKeePassEntry();

            Copy(entry, result);

            return result;
        }

        public static void Copy(this IKeePassEntry source, IKeePassEntry dest)
        {
            dest.Icon = source.Icon;
            dest.Id = source.Id;
            dest.Notes = source.Notes;
            dest.Password = source.Password;
            dest.Title = source.Title;
            dest.Url = source.Url;
            dest.UserName = source.UserName;
            dest.Group = source.Group;

#if DEBUG 
            Debug.Assert(dest.Id.Equals(source.Id));

            foreach (var property in typeof(IKeePassEntry).GetTypeInfo().DeclaredProperties)
            {
                Debug.Assert(Equals(property.GetValue(dest), property.GetValue(source)));
            }
#endif
        }

        public static IKeePassGroup GetGroup(this IKeePassDatabase db, KeePassId id)
        {
            if (id.IsEmpty)
            {
                return db.Root;
            }

            foreach (var entry in db.EnumerateAllGroups())
            {
                if (entry.Id.Equals(id))
                {
                    return entry;
                }
            }

            Debug.Assert(false, "Must find a group in the database");

            return null;
        }

        public static IEnumerable<IKeePassEntry> EnumerateAllEntries(this IKeePassDatabase db)
        {
            return db.Root.EnumerateAllEntries();
        }

        public static IEnumerable<IKeePassGroup> EnumerateAllGroups(this IKeePassDatabase db)
        {
            return db.Root.EnumerateAllGroups(true);
        }

        public static IEnumerable<IKeePassGroup> EnumerateAllGroups(this IKeePassGroup group, bool includeSelf = false)
        {
            if (includeSelf)
            {
                yield return group;
            }

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
                foreach (var item in subgroup.EnumerateAllEntries())
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<IKeePassGroup> EnumerateParents(this IKeePassGroup group, bool includeSelf = false)
        {
            if (group == null)
            {
                yield break;
            }

            if (includeSelf)
            {
                yield return group;
            }

            while (group != null && group.Parent != null)
            {
                yield return group.Parent;

                group = group.Parent;
            }
        }
    }
}
