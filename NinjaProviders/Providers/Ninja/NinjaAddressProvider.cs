﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Providers.BlockChainReader;
using Providers.Contracts.Ninja;
using Providers.TransportTypes.Ninja;

namespace Providers.Providers.Ninja
{
    public class NinjaAddressAliases
    {
        public string ColoredAddress { get; set; }
        public string UncoloredAddress { get; set; }
    }

    public class NinjaAddressSummary
    {
        public NinjaAddressBalance Confirmed { get; set; }
        
        public NinjaAddressBalance Unconfirmed { get; set; }

        public class NinjaAddressBalance
        {
            public NinjaAddressBalance()
            {
                Assets = Enumerable.Empty<NinjaAddressAssetSummary>();
            }

            public int TotalTransactions { get; set; }

            public long Balance { get; set; }

            public IEnumerable<NinjaAddressAssetSummary> Assets { get; set; }

            public class NinjaAddressAssetSummary
            {
                public string AssetId { get; set; }
                public long Quantity { get; set; }
                public long Received { get; set; }
            }

            public static NinjaAddressBalance Create(AddressSummaryContract.AddressSummaryInnerContract source)
            {
                if (source != null)
                {
                    return new NinjaAddressBalance
                    {
                        Balance = source.Balance,
                        TotalTransactions = source.TotalTransactions,
                        Assets = (source.Assets ?? Enumerable.Empty<AddressSummaryContract.AddressSummaryInnerContract.AddressAssetContract>())
                           .Select(p => new NinjaAddressAssetSummary
                           {
                               AssetId = p.AssetId,
                               Quantity = p.Quantity,
                               Received = p.Received
                           })
                    };
                }

                return null;
            }
        }
    }
    
    public class NinjaAddressTransactionList
    {
        public IEnumerable<NinjaAddressTransaction> Transactions { get; set; }

        public class NinjaAddressTransaction
        {
            public string TxId { get; set; }
        }
    }

    public class NinjaAddressProvider
    {
        private readonly NinjaBlockChainReader _blockChainReader;

        public NinjaAddressProvider(NinjaBlockChainReader blockChainReader)
        {
            _blockChainReader = blockChainReader;
        }
        
        public async Task<NinjaAddressAliases> GetAliases(string id)
        {
            var result = await _blockChainReader.GetAsync<WhatIsItAdrressContract>($"/whatisit/{id}");

            if (result != null)
            {
                return new NinjaAddressAliases
                {
                    ColoredAddress = result.ColoredAddress,
                    UncoloredAddress = result.UncoloredAddress
                };
            }

            return null;
        }

        public async Task<NinjaAddressTransactionList> GetTransactionsForAddressAsync(string id)
        {
            var result =  await _blockChainReader.GetAsync<AddressTransactionListContract>($"/balances/{id}");

            if (result != null)
            {
                var tx = result.Transactions ?? Enumerable.Empty<AddressTransactionListItemContract>();
                return new NinjaAddressTransactionList
                {
                    Transactions = tx
                    .Select(p => new NinjaAddressTransactionList.NinjaAddressTransaction
                    {
                        TxId = p.TxId
                    })
                };
            }

            return null;
        }

        public async Task<NinjaAddressSummary> GetAddressBalanceAsync(string id)
        {
            var result = await _blockChainReader.GetAsync<AddressSummaryContract>($"/balances/{id}/summary?colored=true");

            if (result != null)
            {
                return new NinjaAddressSummary
                {
                    Confirmed = NinjaAddressSummary.NinjaAddressBalance.Create(result.Confirmed),
                    Unconfirmed = NinjaAddressSummary.NinjaAddressBalance.Create(result.Unconfirmed)
                };
            }

            return null;
        }
    }
}
