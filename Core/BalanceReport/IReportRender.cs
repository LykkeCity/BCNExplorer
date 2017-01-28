using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
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

    public interface IClientBalances
    {
        IDictionary<string, IEnumerable<IAssetBalance>> AddressBalances { get; } 
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

    public class ClientBalance : IClientBalances
    {
        public ClientBalance()
        {
            AddressBalances = new Dictionary<string, IEnumerable<IAssetBalance>>();    
        }

        public IDictionary<string, IEnumerable<IAssetBalance>> AddressBalances { get; set; }

        public static ClientBalance Create()
        {
            return new ClientBalance();
        }

        public void Add(string address, IEnumerable<IAssetBalance> assetBalances)
        {
            AddressBalances[address] = assetBalances;
        }
    }

   



    public interface IClient
    {
        string Email { get; }
        string Name { get; }
    }

    public class Client:IClient
    {
        public string Email { get; set; }
        public string Name { get; set; }

        public static Client Create(string clientId, string clientName)
        {
            return new Client
            {
                Email = clientId,
                Name = clientName
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
            IClientBalances balances,
            IDictionary<string, IAssetDefinition> assetDefinitions);
    }
}
