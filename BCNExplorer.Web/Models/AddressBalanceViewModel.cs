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
        public IEnumerable<ColoredBalance> Assets { get; set; }
        public bool TotalTransactionsCountCalculated { get; set; }
        public AssetDictionary AssetDic { get; set; }
        public OffchainChannelsByAsset OffchainChannelsByAsset { get; set; }
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
            IBlockHeader atBlock,
            IEnumerable<IChannel> channels)

        {
            return new AddressBalanceViewModel
            {
                AddressId = balance.AddressId,
                TotalConfirmedTransactions = balance.TotalTransactions,
                Balance = balance.BtcBalance,
                Assets = (balance.ColoredBalances ?? Enumerable.Empty<IColoredBalance>()).Select(p => ColoredBalance.Create(p, assetDictionary)),
                UnconfirmedBalanceDelta = balance.UnconfirmedBalanceDelta,
                AssetDic = AssetDictionary.Create(assetDictionary),
                LastBlockHeight = lastBlock.Height,
                LastBlockDateTime = lastBlock.Time,
                AtBlockHeight = (atBlock ?? lastBlock).Height,
                AtBlockDateTime = (atBlock ?? lastBlock).Time,
                OffchainChannelsByAsset = OffchainChannelsByAsset.Create(channels, assetDictionary),
                TotalTransactionsCountCalculated = balance.TotalTransactionsCountCalculated
            };
        }
        
        public class ColoredBalance
        {
            public string AssetId { get; set; }

            public AssetViewModel Asset { get; set; }
            public double Quantity { get; set; }
            public double UnconfirmedQuantityDelta { get; set; }
            public bool ShowUnconfirmedBalance => UnconfirmedQuantityDelta != 0;
            public double UnconfirmedQuantity => Quantity + UnconfirmedQuantityDelta;

            public bool ShowAsset => Quantity != 0 || UnconfirmedQuantity != 0;

            public static ColoredBalance Create(IColoredBalance coloredBalance, IDictionary<string, IAssetDefinition> assetDictionary)
            {
                var asset = assetDictionary.GetValueOrDefault(coloredBalance.AssetId, null);

                var assetViewModel = asset != null
                    ? AssetViewModel.Create(asset)
                    : AssetViewModel.CreateNotFoundAsset(coloredBalance.AssetId);

                return new ColoredBalance
                {
                    AssetId = coloredBalance.AssetId,
                    Quantity = coloredBalance.Quantity,
                    UnconfirmedQuantityDelta = coloredBalance.UnconfirmedQuantityDelta,
                    Asset = assetViewModel,
                };
            }
        }
    }

    public class AddressTransactionsViewModel
    {
        public TransactionIdList AllTransactionIdList { get; set; }
        public TransactionIdList SendTransactionIdList { get; set; }
        public TransactionIdList ReceivedTransactionIdList { get; set; }
        public OffchainChannelPagedList OffchainChannelPagedList { get; set; }
        public bool FullLoaded { get; set; }
        private const int PageSize = 20;

        public static AddressTransactionsViewModel Create(string address, IAddressTransactions source, long offchainChannelsCount, int offchainTransactionsPageSize)
        {
            return new AddressTransactionsViewModel
            {
                AllTransactionIdList = new TransactionIdList(source.All?.Select(p => p.TransactionId), PageSize, false),
                SendTransactionIdList = new TransactionIdList(source.Send?.Select(p => p.TransactionId), PageSize, source.FullLoaded),
                ReceivedTransactionIdList = new TransactionIdList(source.Received?.Select(p => p.TransactionId), PageSize, source.FullLoaded),
                FullLoaded = source.FullLoaded,
                OffchainChannelPagedList = OffchainChannelPagedList.Create(
                    offchainChannelsCount, 
                    offchainTransactionsPageSize, 
                    (url, page)=> url.Action("OffchainChannelPage", "Address", new {address = address, page = page})
                )
            };
        }
    }

    public class OffchainChannelsByAsset
    {
        private ILookup<AssetViewModel, OffchainChannelViewModel> AssetChanneLookup { get; set; }
        private IEnumerable<OffchainChannelViewModel> BtcChannels { get; set; }

        public bool Exists(AssetViewModel asset)
        {
            return Get(asset).Any();
        }

        public IEnumerable<OffchainChannelViewModel> Get(AssetViewModel asset)
        {
            return AssetChanneLookup[asset];
        }

        public bool ExistsBtc()
        {
            return GetBtc().Any();
        }

        public IEnumerable<OffchainChannelViewModel> GetBtc()
        {
            return BtcChannels;
        }

        public static OffchainChannelsByAsset Create(IEnumerable<IChannel> channels, IDictionary<string, IAssetDefinition> assetDictionary)
        {
            return Create(channels.Select(p => OffchainChannelViewModel.Create(p, assetDictionary)));
        }

        public static OffchainChannelsByAsset Create(IEnumerable<OffchainChannelViewModel> channels)
        {
            
            return new OffchainChannelsByAsset
            {
                AssetChanneLookup = channels.Where(p => p.Asset.IsColored).ToLookup(p => p.Asset, AssetViewModel.AssetIdsComparer),
                BtcChannels = channels.Where(p => !p.Asset.IsColored)
            };
        }
    }
}