using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Core.AssetBlockChanges.Mongo;

namespace BCNExplorer.Web.Models
{
    public class AssetTransactionViewModel
    {
        public string TransactionHash { get; set; }

        public static AssetTransactionViewModel Create(IBalanceTransaction source)
        {
            return new AssetTransactionViewModel
            {
                TransactionHash = source.Hash
            };
        }
    }
}