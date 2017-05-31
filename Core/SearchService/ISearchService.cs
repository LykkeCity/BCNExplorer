using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.SearchService
{
    public enum SearchResultType
    {
        Block,
        Transaction,
        OffchainTransaction,
        Address,
        Asset
    }

    public interface ISearchService
    {
        Task<SearchResultType?> GetTypeAsync(string id);
    }
}
