﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using Core.Asset;
using Core.Block;

namespace Core.BalanceReport
{
    public interface IAssetBalance
    {
        decimal Quantity { get; }

        string AssetId { get; }
    }

    public class AssetBalance : IAssetBalance
    {
        public decimal Quantity { get; set; }
        public string AssetId { get; set; }

        public static AssetBalance Create(decimal quantity, string assetId)
        {
            return new AssetBalance
            {
                AssetId = assetId,
                Quantity = quantity
            };
        }
    }



    public interface IClient
    {
        string ClientEmail { get; }
        string Address { get; }
        string BtcExplolerUrl { get; }
    }

    public class Client:IClient
    {
        public string ClientEmail { get; set; }
        public string Address { get; set; }
        public string BtcExplolerUrl { get; set; }

        public static Client Create(string clientId, string address, string btcExplolerUrl)
        {
            return new Client
            {
                Address = address,
                ClientEmail = clientId,
                BtcExplolerUrl = btcExplolerUrl
            };
        }
    }

    public interface IFiatPrices
    {
        string CurrencyName { get; }

        IEnumerable<IAssetMarketPrice> AssetMarketPrices { get; }
    }

    public interface IAssetMarketPrice
    {
        string AssetId { get; }
        decimal MarketPrice { get; }
    }

    public class FiatPrice:IFiatPrices
    {
        public string CurrencyName { get; set; }
        public IEnumerable<IAssetMarketPrice> AssetMarketPrices { get; set; }

        public static FiatPrice Create(string currencyName, IDictionary<string, decimal> priceDictionary)
        {
            return new FiatPrice
            {
                CurrencyName = currencyName,
                AssetMarketPrices = priceDictionary?.Select(p=> AssetMarketPrice.Create(p.Key, p.Value))
            };
        }
    }

    public class AssetMarketPrice : IAssetMarketPrice
    {
        public string AssetId { get; set; }
        public decimal MarketPrice { get; set; }

        public static AssetMarketPrice Create(string assetId, decimal marketPrice)
        {
            return new AssetMarketPrice
            {
                AssetId = assetId,
                MarketPrice = marketPrice
            };
        }
    }

    public interface IReportRender
    {
        void RenderBalance(Stream outputStream, IClient client, 
            IBlockHeader reportedAtBlock, 
            IFiatPrices fiatPrices, 
            IEnumerable<IAssetBalance> balances,
            IDictionary<string, IAssetDefinition> assetDefinitions);
    }
}
