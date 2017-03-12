using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Core.AddressService;
using Core.Settings;
using Core.TransactionCache;
using NBitcoin;
using Providers.Helpers;
using Providers.Providers.Ninja;
using Services.MainChain;
using IAddressTransaction = Core.TransactionCache.IAddressTransaction;

namespace Services.Address
{
    public class AddressMainInfo : IAddressMainInfo
    {
        public string AddressId { get; set; }
        public string UncoloredAddress { get; set; }
        public string ColoredAddress { get; set; }
        public bool IsColored { get; set; }
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
        public bool FullLoaded { get; set; }

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
        private readonly BaseSettings _baseSettings;

        public AddressService(NinjaAddressProvider ninjaAddressProvider, 
            ITransactionCacheItemRepository transactionCacheItemRepository, 
            ITransactionCacheStatusRepository transactionCacheStatusRepository, 
            CachedMainChainService cachedMainChainService, 
            BaseSettings baseSettings)
        {
            _ninjaAddressProvider = ninjaAddressProvider;
            _transactionCacheItemRepository = transactionCacheItemRepository;
            _transactionCacheStatusRepository = transactionCacheStatusRepository;
            _cachedMainChainService = cachedMainChainService;
            _baseSettings = baseSettings;
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
            if (BitcoinAddressHelper.IsBitcoinColoredAddress(id, _baseSettings.UsedNetwork()))
            {
                var result = new BitcoinColoredAddress(id, _baseSettings.UsedNetwork());

                return new AddressMainInfo
                {
                    AddressId = id,
                    ColoredAddress = result.ToWif(),
                    UncoloredAddress = result.Address.ToWif(),
                    IsColored = true
                };
            }

            if (BitcoinAddressHelper.IsBitcoinPubKeyAddress(id, _baseSettings.UsedNetwork()))
            {
                var result = new BitcoinPubKeyAddress(id, _baseSettings.UsedNetwork());

                return new AddressMainInfo
                {
                    AddressId = id,
                    ColoredAddress = result.ToColoredAddress().ToWif(),
                    UncoloredAddress = result.ToWif(),
                    IsColored = false
                };
            }

            if (BitcoinAddressHelper.IsBitcoinScriptAddress(id, _baseSettings.UsedNetwork()))
            {
                var result = new BitcoinScriptAddress(id, _baseSettings.UsedNetwork());

                return new AddressMainInfo
                {
                    AddressId = id,
                    ColoredAddress = result.ToColoredAddress().ToWif(),
                    UncoloredAddress = result.ToWif(),
                    IsColored = false
                };
            }

            return null;
        }

        public async Task<IAddressTransactions> GetTransactions(string id)
        {
            var mainChain = _cachedMainChainService.GetMainChainAsync();
            var cacheStatus = _transactionCacheStatusRepository.GetAsync(id);

            await Task.WhenAll(mainChain, cacheStatus);

            var cacheIsExpired = cacheStatus.Result == null ||
                                 cacheStatus.Result.BlockHeight < mainChain.Result.Tip.Height;
            
            var cachedTxs = cacheStatus.Result != null ? 
                _transactionCacheItemRepository.GetAsync(id) : 
                Task.FromResult(Enumerable.Empty<IAddressTransaction>());

            var notCachedTxsResp = cacheIsExpired ? 
                _ninjaAddressProvider.GetTransactionsForAddressAsync(id, until: cacheStatus.Result?.BlockHeight) : 
                Task.FromResult(NinjaAddressTransactionsResponce.CreateMock(cacheStatus.Result?.FullLoaded ?? true));

            await Task.WhenAll(cachedTxs, notCachedTxsResp);

            var allTx = notCachedTxsResp.Result.Transactions.Union(cachedTxs.Result).ToList();

            if (cacheIsExpired && notCachedTxsResp.Result.Transactions.Any())
            {
                var setStatus = _transactionCacheStatusRepository.SetAsync(id, mainChain.Result.Tip.Height, notCachedTxsResp.Result.FullLoaded);
                var updateData = _transactionCacheItemRepository.SetAsync(id, allTx);

                await Task.WhenAll(setStatus, updateData);
            }

            return new AddressTransactions
            {
                All = allTx.Select(AddressTransaction.Create).Distinct(AddressTransaction.TransactionIdComparer),
                Received = allTx.Where(p => p.IsReceived).Select(AddressTransaction.Create).Distinct(AddressTransaction.TransactionIdComparer),
                Send = allTx.Where(p => !p.IsReceived).Select(AddressTransaction.Create).Distinct(AddressTransaction.TransactionIdComparer),
                FullLoaded = notCachedTxsResp.Result.FullLoaded
            };
        }
    }
}
