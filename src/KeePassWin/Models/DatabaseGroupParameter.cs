using Newtonsoft.Json;

namespace KeePass.Models
{
    internal class DatabaseGroupParameter
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
            return JsonConvert.DeserializeObject<DatabaseGroupParameter>(entry);
        }

        public static string Encode(KeePassId database, KeePassId group)
        {
            return new DatabaseGroupParameter(database, group).ToString();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
