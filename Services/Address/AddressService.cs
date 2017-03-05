using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.AddressService;
using Core.TransactionCache;
using Providers.Providers.Ninja;

namespace Services.Address
{

    public class AddressMainInfo : IAddressMainInfo
    {
        public string AddressId { get; set; }
        public int TotalTransactions { get; set; }
        public string UncoloredAddress { get; set; }
        public string ColoredAddress { get; set; }
    }
    public class AddressBalance:IAddressBalance
    {
        public string AddressId { get; set; }
        public int TotalTransactions { get; set; }
        public double BtcBalance { get; set; }
        public double UnconfirmedBalanceDelta { get; set; }
        public IEnumerable<IColoredBalance> ColoredBalances { get; set; }

        public AddressBalance()
        {
            ColoredBalances = Enumerable.Empty<IColoredBalance>();
        }
    }

    public class AddressTransactions : IAddressTransactions
    {
        public IEnumerable<IAddressTransaction> All { get; set; }
        public IEnumerable<IAddressTransaction> Send { get; set; }
        public IEnumerable<IAddressTransaction> Received { get; set; }

        public AddressTransactions()
        {
            All = Enumerable.Empty<IAddressTransaction>();
            Send = Enumerable.Empty<IAddressTransaction>();
            Received = Enumerable.Empty<IAddressTransaction>();
        }
    }

    public class ColoredBalance:IColoredBalance
    {
        public string AssetId { get; set; }
        public double Quantity { get; set; }
        public double UnconfirmedQuantityDelta { get; set; }
    }

    public class AddressTransaction : IAddressTransaction
    {
        private sealed class TransactionIdEqualityComparer : IEqualityComparer<AddressTransaction>
        {
            public bool Equals(AddressTransaction x, AddressTransaction y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.TransactionId, y.TransactionId);
            }

            public int GetHashCode(AddressTransaction obj)
            {
                return (obj.TransactionId != null ? obj.TransactionId.GetHashCode() : 0);
            }
        }

        private static readonly IEqualityComparer<AddressTransaction> TransactionIdComparerInstance = new TransactionIdEqualityComparer();

        public static IEqualityComparer<AddressTransaction> TransactionIdComparer
        {
            get { return TransactionIdComparerInstance; }
        }

        public string TransactionId { get; set; }

        public static AddressTransaction Create(ITransactionCacheItem source)
        {
            return new AddressTransaction
            {
                TransactionId = source.TransactionId
            };
        }
    }


    public class AddressService:IAddressService
    {
        private readonly NinjaAddressProvider _ninjaAddressProvider;
        private readonly ITransactionCacheRepository _transactionCacheRepository;

        public AddressService(NinjaAddressProvider ninjaAddressProvider, 
            ITransactionCacheRepository transactionCacheRepository)
        {
            _ninjaAddressProvider = ninjaAddressProvider;
            _transactionCacheRepository = transactionCacheRepository;
        }

        public async Task<IAddressBalance> GetBalanceAsync(string id, int? at = null)
        {
            var coloredSummary =  await _ninjaAddressProvider.GetAddressBalanceAsync(id, at, colored: true);

            if (coloredSummary != null )
            {
                var result = new AddressBalance
                {
                    AddressId = id,
                    BtcBalance = coloredSummary.Confirmed.Balance,
                    TotalTransactions = coloredSummary.Confirmed.TotalTransactions,
                    UnconfirmedBalanceDelta = coloredSummary.Unconfirmed?.Balance ?? 0
                };
                var unconfirmedAssets = coloredSummary.Unconfirmed?.Assets ?? Enumerable.Empty<NinjaAddressSummary.NinjaAddressBalance.NinjaAddressAssetSummary>();

                foreach (var assetSummary in unconfirmedAssets.Where(p => !coloredSummary.Confirmed.Assets.Select(x=>x.AssetId).Contains(p.AssetId))) //assets with 0
                {
                    coloredSummary.Confirmed.Assets.Add(new NinjaAddressSummary.NinjaAddressBalance.NinjaAddressAssetSummary
                    {
                        AssetId = assetSummary.AssetId
                    });
                }

                result.ColoredBalances = coloredSummary.Confirmed.Assets.Select(p =>
                {
                    var coloredBalance = new ColoredBalance
                    {
                        AssetId = p.AssetId,
                        Quantity = p.Quantity
                    };
                    var unconfirmedAsset = unconfirmedAssets?.FirstOrDefault(ua => ua.AssetId == coloredBalance.AssetId);
                    if (unconfirmedAsset != null)
                    {
                        coloredBalance.UnconfirmedQuantityDelta = unconfirmedAsset.Quantity;
                    }

                    return coloredBalance;
                });
                
                return result;
            }

            return null;
        }

        public async Task<IAddressMainInfo> GetMainInfoAsync(string id)
        {
            var aliases = await _ninjaAddressProvider.GetAliases(id);
            if (aliases != null)
            {
                return new AddressMainInfo
                {
                    AddressId = id,
                    ColoredAddress = aliases.ColoredAddress,
                    UncoloredAddress = aliases.UncoloredAddress
                };
            }
            return null;
        }

        public async Task<IAddressTransactions> GetTransactions(string id)
        {
            var lastCachedTx = await _transactionCacheRepository.GetLastCachedTransaction(id);

            var cachedTxs = _transactionCacheRepository.GetAsync(id);
            var notCachedTxs = _ninjaAddressProvider.GetTransactionsForAddressAsync(id, until: lastCachedTx?.BlockHeight - 1);

            await Task.WhenAll(cachedTxs, notCachedTxs);

            var notCachedAllTxs = notCachedTxs.Result.AllTransactions.Select(AddressTransaction.Create);
            var notCachedReceivedTxs = notCachedTxs.Result.AllTransactions.Where(p => p.IsReceived).Select(AddressTransaction.Create);
            var notCachedSendTxs = notCachedTxs.Result.AllTransactions.Where(p => !p.IsReceived).Select(AddressTransaction.Create);

            var cachedAllTxs = cachedTxs.Result.Select(AddressTransaction.Create);
            var cachedReceivedTxs = cachedTxs.Result.Where(p => p.IsReceived).Select(AddressTransaction.Create);
            var cachedSendTxs = cachedTxs.Result.Where(p => !p.IsReceived).Select(AddressTransaction.Create);

            await _transactionCacheRepository.InsertOrReplaceAsync(notCachedTxs.Result.AllTransactions);

            return new AddressTransactions
            {
                All = notCachedAllTxs.Union(cachedAllTxs).Distinct(AddressTransaction.TransactionIdComparer),
                Received = notCachedReceivedTxs.Union(cachedReceivedTxs).Distinct(AddressTransaction.TransactionIdComparer),
                Send = notCachedSendTxs.Union(cachedSendTxs).Distinct(AddressTransaction.TransactionIdComparer)
            };
        }
    }
}
