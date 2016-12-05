using System;
using System.Collections.Generic;
using System.Linq;
using Core.AddressService;
using Core.Asset;

namespace BCNExplorer.Web.Models
{
    public class AddressViewModel
    {
        public string AddressId { get; set; }
        public string UncoloredAddress { get; set; }
        public string ColoredAddress { get; set; }
        public double Balance { get; set; }
        public double UnconfirmedBalance => Balance + UnconfirmedBalanceDelta;
        public double UnconfirmedBalanceDelta { get; set; }
        public bool ShowUnconfirmedBalance => UnconfirmedBalanceDelta != 0;
        public double TotalConfirmedTransactions { get; set; }
        public IEnumerable<Asset> Assets { get; set; }
        public TransactionIdList TransactionIdList { get; set; }
        private const int PageSize = 20;

        public AssetDictionary AssetDic { get; set; }

        public static AddressViewModel Create(IAddressBalance balance, IDictionary<string, IAssetDefinition> assetDictionary)
        {
            return new AddressViewModel
            {
                AddressId = balance.AddressId,
                TransactionIdList = new TransactionIdList(balance.TransactionIds, PageSize),
                UncoloredAddress = balance.UncoloredAddress,
                ColoredAddress = balance.ColoredAddress,
                TotalConfirmedTransactions = balance.TotalTransactions,
                Balance = balance.Balance,
                Assets = (balance.ColoredBalances??Enumerable.Empty<IColoredBalance>()).Select(p => new Asset
                {
                    AssetId = p.AssetId,
                    Quantity = p.Quantity,
                    UnconfirmedQuantityDelta = p.UnconfirmedQuantityDelta
                }),
                UnconfirmedBalanceDelta = balance.UnconfirmedBalanceDelta,
                AssetDic = AssetDictionary.Create(assetDictionary)
            };
        }

        public class Asset
        {
            public string AssetId { get; set; }
            public double Quantity { get; set; }
            public double UnconfirmedQuantityDelta { get; set; }
            public bool ShowUnconfirmedBalance => UnconfirmedQuantityDelta != 0;
            public double UnconfirmedQuantity => Quantity + UnconfirmedQuantityDelta;
        }
    }
}