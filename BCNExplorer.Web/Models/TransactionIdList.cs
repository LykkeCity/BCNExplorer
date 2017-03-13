using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BCNExplorer.Web.Models
{
    public class TransactionIdList
    {
        private const int MaxItemsToShow = 1000;
        private readonly IEnumerable<string> _transactionIds;

        public readonly int _pageSize;
        public bool FullLoaded { get; set; }

        public TransactionIdList(IEnumerable<string> source, int pageSize = 20, bool fullLoaded = true)
        {
            _transactionIds = source ?? Enumerable.Empty<string>();
            _pageSize = pageSize;
            FullLoaded = fullLoaded;
        }
        
        public int TotalItems => _transactionIds.Count();
        public int TotalPages => (int)Math.Ceiling(((double)TotalItems) / _pageSize);
        public int CurrentPage = 1;

        public IEnumerable<string> GetPage(int page)
        {
            return _transactionIds.Skip((page - 1)*_pageSize).Take(_pageSize);
        }

        private IEnumerable<int> GetPages()
        {
            if (TotalItems <= MaxItemsToShow)
            {
                return Enumerable.Range(1, TotalPages);
            }
            else
            {
                return Enumerable.Range(1, (int)Math.Ceiling(((double)MaxItemsToShow) / _pageSize));
            }
        }

        public IEnumerable<int> Pages => GetPages();
    }
}