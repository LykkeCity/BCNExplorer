using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Core.AddressService;
using Core.Asset;
using Core.Block;
using Core.Channel;

namespace BCNExplorer.Web.Models
{
    public class AddressBalanceViewModel
    {
        public string AddressId { get; set; }
        public double Balance { get; set; }
        public double UnconfirmedBalance => Balance + UnconfirmedBalanceDelta;
        public double UnconfirmedBalanceDelta { get; set; }
        public bool ShowUnconfirmedBalance => UnconfirmedBalanceDelta != 0;
        public double TotalConfirmedTransactions { get; set; }
        public IEnumerable<Asset> Assets { get; set; }
        public AssetDictionary AssetDic { get; set; }
        public OffchainAssetDictionary OffchainAssetDictionary { get; set; }
        public DateTime LastBlockDateTime { get; set; }
        public int LastBlockHeight { get; set; }

        public DateTime AtBlockDateTime { get; set; }
        public int AtBlockHeight { get; set; }

        public bool ShowNext => NextBlock <= LastBlockHeight;
        public bool ShowPrev => PrevBlock >= 0;
        public int PrevBlock => AtBlockHeight - 1;
        public int NextBlock => AtBlockHeight + 1;
        
        public static AddressBalanceViewModel Create(IAddressBalance balance, 
            IDictionary<string, IAssetDefinition> assetDictionary, 
            IBlockHeader lastBlock, 
            IBlockHeader atBlock)
        {
            return new AddressBalanceViewModel
            {
                AddressId = balance.AddressId,
                TotalConfirmedTransactions = balance.TotalTransactions,
                Balance = balance.BtcBalance,
                Assets = (balance.ColoredBalances??Enumerable.Empty<IColoredBalance>()).Select(p => new Asset
                {
                    AssetId = p.AssetId,
                    Quantity = p.Quantity,
                    UnconfirmedQuantityDelta = p.UnconfirmedQuantityDelta
                }),
                UnconfirmedBalanceDelta = balance.UnconfirmedBalanceDelta,
                AssetDic = AssetDictionary.Create(assetDictionary),
                LastBlockHeight = lastBlock.Height,
                LastBlockDateTime = lastBlock.Time,
                AtBlockHeight = (atBlock ?? lastBlock).Height,
                AtBlockDateTime = (atBlock ?? lastBlock).Time,
            };
        }
        
        public class Asset
        {
            public string AssetId { get; set; }
            public double Quantity { get; set; }
            public double UnconfirmedQuantityDelta { get; set; }
            public bool ShowUnconfirmedBalance => UnconfirmedQuantityDelta != 0;
            public double UnconfirmedQuantity => Quantity + UnconfirmedQuantityDelta;

            public bool ShowAsset => Quantity != 0 || UnconfirmedQuantity != 0;
        }
    }

    public class AddressTransactionsViewModel
    {
        public TransactionIdList AllTransactionIdList { get; set; }
        public TransactionIdList SendTransactionIdList { get; set; }
        public TransactionIdList ReceivedTransactionIdList { get; set; }
        public OffchainChannelListViewModel OffchainChannelList { get; set; }
        public bool FullLoaded { get; set; }
        private const int PageSize = 20;

        public static AddressTransactionsViewModel Create(IAddressTransactions source, 
            IEnumerable<IFilledChannel> channels, 
            IDictionary<string, IAssetDefinition> assetDictionary)
        {
            return new AddressTransactionsViewModel
            {
                AllTransactionIdList = new TransactionIdList(source.All?.Select(p => p.TransactionId), PageSize, source.FullLoaded),
                SendTransactionIdList = new TransactionIdList(source.Send?.Select(p => p.TransactionId), PageSize, source.FullLoaded),
                ReceivedTransactionIdList = new TransactionIdList(source.Received?.Select(p => p.TransactionId), PageSize, source.FullLoaded),
                FullLoaded = source.FullLoaded,
                OffchainChannelList = OffchainChannelListViewModel.Create(channels, assetDictionary)
            };
        }
    }

    public class OffchainAssetDictionary
    {
        private IDictionary<string, OffchainChannelViewModel> AssetChannelDictionary { get; set; }
        private IEnumerable<OffChainTransactionViewModel> BtcChannels { get; set; }

        public static OffchainAssetDictionary Create(IEnumerable<IFilledChannel> channels, IDictionary<string, IAssetDefinition> assetDictionary)
        {
            return Create(channels.Select(p => OffchainChannelViewModel.Create(p, assetDictionary)));
        }

        public static OffchainAssetDictionary Create(IEnumerable<OffchainChannelViewModel> channels)
        {
            throw new Exception();
            //return Create(channels.SelectMany(p => p.OffChainTransactions));
        }

        //public OffChainTransactionViewModel GetForBtc()
        //{
        //    return BtcTransaction;
        //}

        //public OffChainTransactionViewModel GetFor(string assetId)
        //{
        //    return AssetDictionary.GetValueOrDefault(assetId, null);
        //}

        //public static OffchainAssetDictionary Create(IEnumerable<OffChainTransactionViewModel> transactions)
        //{
        //    var confirmedTxs = transactions.Where(p => !p.IsRevoked).ToList();

        //    var assetDic = new Dictionary<string, OffChainTransactionViewModel>();

        //    foreach (var tx in confirmedTxs)
        //    {
        //        foreach (var assetId in tx.Asset.AssetIds)
        //        {
        //            assetDic[assetId] = tx;
        //        }
        //    }

        //    var btcTransaction = confirmedTxs.FirstOrDefault(p => !p.Asset.IsColored);

        //    return new OffchainAssetDictionary
        //    {
        //        AssetDictionary = assetDic,
        //        BtcTransaction = btcTransaction
        //    };
        //}
    }


}