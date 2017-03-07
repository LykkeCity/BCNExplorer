using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.AddressService;
using Core.TransactionCache;
using Providers.Providers.Ninja;
using Services.MainChain;
using IAddressTransaction = Core.TransactionCache.IAddressTransaction;

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
        public IEnumerable<Core.AddressService.IAddressTransaction> All { get; set; }
        public IEnumerable<Core.AddressService.IAddressTransaction> Send { get; set; }
        public IEnumerable<Core.AddressService.IAddressTransaction> Received { get; set; }

        public AddressTransactions()
        {
            All = Enumerable.Empty<Core.AddressService.IAddressTransaction>();
            Send = Enumerable.Empty<Core.AddressService.IAddressTransaction>();
            Received = Enumerable.Empty<Core.AddressService.IAddressTransaction>();
        }
    }

    public class ColoredBalance:IColoredBalance
    {
        public string AssetId { get; set; }
        public double Quantity { get; set; }
        public double UnconfirmedQuantityDelta { get; set; }
    }

    public class AddressTransaction : Core.AddressService.IAddressTransaction
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

        public static AddressTransaction Create(IAddressTransaction source)
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
        private readonly ITransactionCacheItemRepository _transactionCacheItemRepository;
        private readonly ITransactionCacheStatusRepository _transactionCacheStatusRepository;
        private readonly CachedMainChainService _cachedMainChainService;

        public AddressService(NinjaAddressProvider ninjaAddressProvider, 
            ITransactionCacheItemRepository transactionCacheItemRepository, 
            ITransactionCacheStatusRepository transactionCacheStatusRepository, 
            CachedMainChainService cachedMainChainService)
        {
            _ninjaAddressProvider = ninjaAddressProvider;
            _transactionCacheItemRepository = transactionCacheItemRepository;
            _transactionCacheStatusRepository = transactionCacheStatusRepository;
            _cachedMainChainService = cachedMainChainService;
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
            var mainChain = _cachedMainChainService.GetMainChainAsync();
            var lastCached = _transactionCacheStatusRepository.GetAsync(id);

            await Task.WhenAll(mainChain, lastCached);

            var cacheIsExpired = lastCached.Result == null ||
                                 lastCached.Result.BlockHeight < mainChain.Result.Tip.Height;


            var cachedTxs = lastCached.Result != null ? 
                _transactionCacheItemRepository.GetAsync(id) : 
                Task.FromResult(Enumerable.Empty<IAddressTransaction>());

            var notCachedTxs = cacheIsExpired ? 
                _ninjaAddressProvider.GetTransactionsForAddressAsync(id, until: lastCached.Result?.BlockHeight) : 
                Task.FromResult(Enumerable.Empty<IAddressTransaction>());

            await Task.WhenAll(cachedTxs, notCachedTxs);

            var allTx = notCachedTxs.Result.Union(cachedTxs.Result).ToList();

            if (cacheIsExpired && notCachedTxs.Result.Any())
            {
                var setStatus = _transactionCacheStatusRepository.SetAsync(id, mainChain.Result.Tip.Height);
                var updateData = _transactionCacheItemRepository.SetAsync(id, allTx);

                await Task.WhenAll(setStatus, updateData);
            }

            return new AddressTransactions
            {
                All = allTx.Select(AddressTransaction.Create).Distinct(AddressTransaction.TransactionIdComparer),
                Received = allTx.Where(p => p.IsReceived).Select(AddressTransaction.Create).Distinct(AddressTransaction.TransactionIdComparer),
                Send = allTx.Where(p => !p.IsReceived).Select(AddressTransaction.Create).Distinct(AddressTransaction.TransactionIdComparer)
            };
        }
    }
}
