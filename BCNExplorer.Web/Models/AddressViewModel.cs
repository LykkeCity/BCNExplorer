using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Core.Asset;
using Providers.TransportTypes;
using Providers.TransportTypes.Asset;
using Providers.TransportTypes.Ninja;

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

        public static AddressViewModel Create(NinjaAddress ninjaAddress, IDictionary<string, IAsset> assetDictionary)
        {
            return new AddressViewModel
            {
                AddressId = ninjaAddress.AddressId,
                TransactionIdList = new TransactionIdList(ninjaAddress.TransactionIds, PageSize),
                UncoloredAddress = ninjaAddress.UncoloredAddress,
                ColoredAddress = ninjaAddress.ColoredAddress,
                TotalConfirmedTransactions = ninjaAddress.TotalTransactions,
                Balance = ninjaAddress.Balance,
                Assets = (ninjaAddress.Assets ?? Enumerable.Empty<NinjaAddress.Asset>()).Select(p => new Asset
                {
                    AssetId = p.AssetId,
                    Quantity = p.Quantity,
                    UnconfirmedQuantityDelta = p.UnconfirmedQuantityDelta
                }),
                UnconfirmedBalanceDelta = ninjaAddress.UnconfirmedBalanceDelta,
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