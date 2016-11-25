using System;

namespace KeePass.Models
{
    internal struct DatabaseGroupParameter
    {
        public KeePassId Database { get; }
        public KeePassId Group { get; }

        public DatabaseGroupParameter(KeePassId database, KeePassId group)
        {
            Database = database;
            Group = group;
        }

        public static DatabaseGroupParameter Parse(string entry)
        {
            var db = Guid.Parse(entry.Substring(0, 32));
            var group = Guid.Parse(entry.Substring(32));

            return new DatabaseGroupParameter(new KeePassId(db), new KeePassId(group));
        }

        public static string Encode(KeePassId database, KeePassId group)
        {
            return database.Id.ToString("N") + group.Id.ToString("N");
        }
    }
}
