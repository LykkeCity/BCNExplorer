namespace Providers.TransportTypes.Ninja
{

    public class SearchResult
    {
        public SearchResultType SearchResultType;
        public string RedirectId { get; set; }

        public bool IsSearchSuccess { get; set; }
    }
    public enum SearchResultType
    {
        Block,
        Transaction,
        Address,
        Asset
    }
}
