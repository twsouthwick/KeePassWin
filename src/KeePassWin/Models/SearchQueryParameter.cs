namespace KeePass.Models
{
    internal class SearchQueryParameter
    {
        public KeePassId Database { get; }
        public string Term { get; }

        public SearchQueryParameter(KeePassId database, string term)
        {
            Database = database;
            Term = term;
        }

        public static SearchQueryParameter Parse(string entry)
        {
            var database = entry.Substring(0, 32);
            var term = entry.Substring(32);

            return new SearchQueryParameter(database, term);
        }

        public static string Encode(KeePassId database, string term)
        {
            return new SearchQueryParameter(database, term).ToString();
        }

        public override string ToString()
        {
            return (string)Database + Term;
        }
    }
}
