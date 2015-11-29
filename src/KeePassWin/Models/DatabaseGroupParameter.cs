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
            var database = entry.Substring(0, 32);
            var group = entry.Substring(32);

            return new DatabaseGroupParameter(database, group);
        }

        public static string Encode(KeePassId database, KeePassId group)
        {
            return new DatabaseGroupParameter(database, group).ToString();
        }

        public override string ToString()
        {
            return (string)Database + (string)Group;
        }
    }
}
