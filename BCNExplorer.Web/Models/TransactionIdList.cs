using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BCNExplorer.Web.Models
{
    public class TransactionIdList
    {
        private readonly IEnumerable<string> _transactionIds;

        public readonly int _pageSize;

        public TransactionIdList(IEnumerable<string> source, int pageSize)
        {
            _transactionIds = source ?? Enumerable.Empty<string>();
            _pageSize = pageSize;
        }


        public int TotalItems => _transactionIds.Count();
        public int TotalPages => (int)Math.Ceiling(((double)TotalItems) / _pageSize);
        public int CurrentPage = 1;

        public IEnumerable<string> GetPage(int page)
        {
            return _transactionIds.Skip((page - 1)*_pageSize).Take(_pageSize);
        }

        public IEnumerable<int> Pages => Enumerable.Range(1, TotalPages);
    }
}