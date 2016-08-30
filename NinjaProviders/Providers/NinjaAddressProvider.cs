using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinjaProviders.BlockChainReader;
using NinjaProviders.Contracts;
using NinjaProviders.TransportTypes;

namespace NinjaProviders.Providers
{
    public class NinjaAddressProvider
    {
        private readonly NinjaBlockChainReader _blockChainReader;

        public NinjaAddressProvider(NinjaBlockChainReader blockChainReader)
        {
            _blockChainReader = blockChainReader;
        }

        public async Task<NinjaAddress> GetAddress(string id)
        {
            NinjaAddress ninjaAddress = null;
            var fillMainInfoTask = GetAddressMainInfo(id).ContinueWith(p =>
            {
                if (p.Result != null)
                {
                    ninjaAddress = ninjaAddress ?? new NinjaAddress();
                    ninjaAddress.AddressId = id;
                    ninjaAddress.ColoredAddress = p.Result.ColoredAddress;
                    ninjaAddress.UncoloredAddress = p.Result.UncoloredAddress;
                }
            });

            var fillTransactionsTask = GetTransactionsForAddress(id).ContinueWith(p =>
            {
                if (p.Result != null)
                {
                    ninjaAddress = ninjaAddress ?? new NinjaAddress();
                    ninjaAddress.TransactionIds = p.Result.Transactions.Select(tr => tr.TxId);
                }
            });

            var fillSummaryTask = GetAddressSummary(id).ContinueWith(p =>
            {
                if (p.Result != null)
                {
                    ninjaAddress = ninjaAddress ?? new NinjaAddress();
                    ninjaAddress.Balance = p.Result.Confirmed.Balance;
                    ninjaAddress.TotalTransactions = p.Result.Confirmed.TotalTransactions;

                    var assets = p.Result.Confirmed.Assets ?? Enumerable.Empty<AddressAssetContract>();
                    ninjaAddress.Assets = assets.Select(a => new NinjaAddress.Asset
                    {
                        AssetId = a.AssetId,
                        Quantity = a.Quantity
                    });
                }
            });

            await Task.WhenAll(fillMainInfoTask, fillTransactionsTask, fillSummaryTask);

            return ninjaAddress;
        } 

        private async Task<WhatIsItAdrressContract> GetAddressMainInfo(string id)
        {
            return await _blockChainReader.DoRequest<WhatIsItAdrressContract>($"/whatisit/{id}");
        }

        private async Task<AddressTransactionListContract> GetTransactionsForAddress(string id)
        {
            return await _blockChainReader.DoRequest<AddressTransactionListContract>($"/balances/{id}");
        }

        private async Task<AddressSummaryContract> GetAddressSummary(string id)
        {
            return await _blockChainReader.DoRequest<AddressSummaryContract>($"/balances/{id}/summary");
        }
    }
}
