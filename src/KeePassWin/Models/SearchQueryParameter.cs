using Newtonsoft.Json;

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
            return JsonConvert.DeserializeObject<SearchQueryParameter>(entry);
        }

        public static string Encode(KeePassId database, string term)
        {
            return new SearchQueryParameter(database, term).ToString();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
