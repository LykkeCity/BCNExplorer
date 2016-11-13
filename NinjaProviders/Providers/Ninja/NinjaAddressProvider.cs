using System;
using System.Linq;
using System.Threading.Tasks;
using Providers.BlockChainReader;
using Providers.Contracts.Ninja;
using Providers.TransportTypes.Ninja;

namespace Providers.Providers.Ninja
{
    public class NinjaAddressProvider
    {
        private readonly NinjaBlockChainReader _blockChainReader;

        public NinjaAddressProvider(NinjaBlockChainReader blockChainReader)
        {
            _blockChainReader = blockChainReader;
        }

        public async Task<NinjaAddress> GetAddressAsync(string id)
        {
            var ninjaAddress = new Lazy<NinjaAddress>( () => new NinjaAddress());
            var fillMainInfoTask = GetAddressMainInfoAsync(id).ContinueWith(p =>
            {
                if (p.Result != null)
                {
                    ninjaAddress.Value.AddressId = id;
                    ninjaAddress.Value.ColoredAddress = p.Result.ColoredAddress;
                    ninjaAddress.Value.UncoloredAddress = p.Result.UncoloredAddress;
                }
            });

            var fillTransactionsTask = GetTransactionsForAddressAsync(id).ContinueWith(p =>
            {
                if (p.Result != null)
                {
                    ninjaAddress.Value.TransactionIds = p.Result.Transactions.Select(tr => tr.TxId);
                }
            });

            var fillSummaryTask = GetAddressBalanceAsync(id).ContinueWith(p =>
            {
                if (p.Result != null)
                {
                    ninjaAddress.Value.Balance = p.Result.Confirmed.Balance;
                    ninjaAddress.Value.TotalTransactions = p.Result.Confirmed.TotalTransactions;

                    ninjaAddress.Value.UnconfirmedBalanceDelta = p.Result.Unconfirmed.Balance;

                    var assets = p.Result.Confirmed.Assets ?? Enumerable.Empty<AddressAssetContract>();
                    ninjaAddress.Value.Assets = assets.Select(a => new NinjaAddress.Asset
                    {
                        AssetId = a.AssetId,
                        Quantity = a.Quantity
                    });
                }
            });

            await Task.WhenAll(fillMainInfoTask, fillTransactionsTask, fillSummaryTask);

            return ninjaAddress.IsValueCreated ? ninjaAddress.Value : null;
        } 

        private async Task<WhatIsItAdrressContract> GetAddressMainInfoAsync(string id)
        {
            return await _blockChainReader.GetAsync<WhatIsItAdrressContract>($"/whatisit/{id}");
        }

        private async Task<AddressTransactionListContract> GetTransactionsForAddressAsync(string id)
        {
            return await _blockChainReader.GetAsync<AddressTransactionListContract>($"/balances/{id}");
        }

        private async Task<AddressSummaryContract> GetAddressBalanceAsync(string id)
        {
            return await _blockChainReader.GetAsync<AddressSummaryContract>($"/balances/{id}/summary?colored=true");
        }
    }
}
