using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.AddressService;
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
        public string TransactionId { get; set; }

        public static AddressTransaction Create(NinjaAddressTransactionList.NinjaAddressTransaction source)
        {
            return new AddressTransaction
            {
                TransactionId = source.TxId
            };
        }
    }


    public class AddressService:IAddressService
    {
        private readonly NinjaAddressProvider _ninjaAddressProvider;

        public AddressService(NinjaAddressProvider ninjaAddressProvider)
        {
            _ninjaAddressProvider = ninjaAddressProvider;
        }

        public async Task<IAddressBalance> GetBalanceAsync(string id, int? at = null)
        {
            var summary = await _ninjaAddressProvider.GetAddressBalanceAsync(id, at);
            if (summary != null)
            {
                var result = new AddressBalance
                {
                    AddressId = id,
                    BtcBalance = summary.Confirmed.Balance,
                    TotalTransactions = summary.Confirmed.TotalTransactions,
                    UnconfirmedBalanceDelta = summary.Unconfirmed?.Balance ?? 0
                };
                var unconfirmedAssets = summary.Unconfirmed?.Assets ?? Enumerable.Empty<NinjaAddressSummary.NinjaAddressBalance.NinjaAddressAssetSummary>();

                foreach (var assetSummary in unconfirmedAssets.Where(p => !summary.Confirmed.Assets.Select(x=>x.AssetId).Contains(p.AssetId))) //assets with 0
                {
                    summary.Confirmed.Assets.Add(new NinjaAddressSummary.NinjaAddressBalance.NinjaAddressAssetSummary
                    {
                        AssetId = assetSummary.AssetId
                    });
                }

                result.ColoredBalances = summary.Confirmed.Assets.Select(p =>
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
            throw new NotImplementedException();
        }

        public async Task<IAddressTransactions> GetTransactions(string id)
        {
            var tx = await _ninjaAddressProvider.GetTransactionsForAddressAsync(id);

            return new AddressTransactions
            {
                All = tx.AllTransactions.Select(AddressTransaction.Create),
                Received = tx.ReceivedTransactions.Select(AddressTransaction.Create),
                Send = tx.SendTransactions.Select(AddressTransaction.Create)
            };
        }
    }
}
