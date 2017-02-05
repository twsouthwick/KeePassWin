using System;

namespace KeePass.Models
{
    public struct DatabaseGroupParameter
    {
        public DatabaseGroupParameter(KeePassId database, KeePassId group)
        {
            Database = database;
            Group = group;
        }

        public KeePassId Database { get; }

        public KeePassId Group { get; }

        public static DatabaseGroupParameter Decode(string encodedParameter)
        {
            if (encodedParameter.Length != 64)
            {
                throw new ArgumentOutOfRangeException(nameof(encodedParameter), encodedParameter, "Encoded parameter must have a length of 64.");
            }

            var db = Guid.Parse(encodedParameter.Substring(0, 32));
            var group = Guid.Parse(encodedParameter.Substring(32));

            return new DatabaseGroupParameter(new KeePassId(db), new KeePassId(group));
        }

        public static string Encode(KeePassId database, KeePassId group)
        {
            return database.Id.ToString("N") + group.Id.ToString("N");
        }
    }
}
