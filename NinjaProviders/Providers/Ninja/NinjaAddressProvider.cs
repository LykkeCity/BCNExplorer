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
            var fillMainInfoTask = GetAddressMainInfoAsync(id).ContinueWith(task =>
            {
                if (task.Result != null)
                {
                    ninjaAddress.Value.AddressId = id;
                    ninjaAddress.Value.ColoredAddress = task.Result.ColoredAddress;
                    ninjaAddress.Value.UncoloredAddress = task.Result.UncoloredAddress;
                }
            });

            var fillTransactionsTask = GetTransactionsForAddressAsync(id).ContinueWith(task =>
            {
                if (task.Result != null)
                {
                    ninjaAddress.Value.TransactionIds = task.Result.Transactions.Select(tr => tr.TxId);
                }
            });

            var fillSummaryTask = GetAddressBalanceAsync(id).ContinueWith(task =>
            {
                if (task.Result != null)
                {
                    ninjaAddress.Value.Balance = task.Result.Confirmed.Balance;
                    ninjaAddress.Value.TotalTransactions = task.Result.Confirmed.TotalTransactions;

                    ninjaAddress.Value.UnconfirmedBalanceDelta = task.Result.Unconfirmed.Balance;
                    var unconfirmedAssets = task.Result.Unconfirmed.Assets ?? Enumerable.Empty<AddressAssetContract>();

                    ninjaAddress.Value.Assets = (task.Result.Confirmed.Assets ?? Enumerable.Empty<AddressAssetContract>())
                        .Select(a =>
                        {
                            var ninjaAsset =  new NinjaAddress.Asset
                            {
                                AssetId = a.AssetId,
                                Quantity = a.Quantity
                            };
                            var unconfirmedAsset = unconfirmedAssets.FirstOrDefault(p => p.AssetId == ninjaAsset.AssetId);
                            if (unconfirmedAsset != null)
                            {
                                ninjaAsset.UnconfirmedQuantityDelta = unconfirmedAsset.Quantity;
                            }
                            return ninjaAsset;
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
