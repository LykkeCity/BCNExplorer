using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Core.TransactionCache;
using NBitcoin;
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
                Assets = Enumerable.Empty<NinjaAddressAssetSummary>().ToList();
            }

            public int TotalTransactions { get; set; }

            public long Balance { get; set; }

            public IList<NinjaAddressAssetSummary> Assets { get; set; }

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
                           }).ToList()
                    };
                }

                return null;
            }
        }
    }


    public class NinjaAddressTransaction : IAddressTransaction
    {
        public string TransactionId { get; set; }
        public bool IsReceived { get; set; }
        public int? BlockHeight { get; set; }
        public string Address { get; set; }

        public static NinjaAddressTransaction Create(string address, AddressTransactionListItemContract source)
        {
            return new NinjaAddressTransaction
            {
                TransactionId = source.TxId,
                IsReceived = source.IsReceived(),
                BlockHeight = source.Height,
                Address = address
            };
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

        public async Task<IEnumerable<IAddressTransaction>> GetTransactionsForAddressAsync(string id, int? until = null, int? from = null)
        {
            var baseUrl = $"/balances/{id}";
            string continuationToken = null;
            var result = new List<IAddressTransaction>();
            do
            {
                var url = baseUrl;
                if (until != null || from != null || continuationToken != null)
                {
                    var queryParams = new
                    {
                        until = until,
                        from = from,
                        continuation = continuationToken
                    };

                    url += "?" + queryParams.ToUrlParamString();
                }

                var resp = await _blockChainReader.GetAsync<AddressTransactionListContract>(url);

                if (resp != null)
                {
                    continuationToken = resp.ContinuationToken;
                    var tx = resp.Transactions ?? Enumerable.Empty<AddressTransactionListItemContract>();
                    result.AddRange(tx.Select(p => NinjaAddressTransaction.Create(id, p)));
                }
                else
                {
                    return null;
                }
            } while (!string.IsNullOrEmpty(continuationToken));

            return result;
        }


        public async Task<NinjaAddressSummary> GetAddressBalanceAsync(string id, int? at, bool colored)
        {
            if (!colored)
            {
                //getting uncolored address if id is colored
                try
                {
                    id = new BitcoinColoredAddress(id).Address.ToWif();
                }
                catch (Exception)
                {
                    //address is uncolored already - its ok
                }
            }
            var url = $"/balances/{id}/summary?colored={colored}";
            if (at != null)
            {
                url += $"&at={at.Value}";
            }

            var result = await _blockChainReader.GetAsync<AddressSummaryContract>(url);

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

    static class AddressTransactionListItemContractHelper
    {
        public static bool IsSend(this AddressTransactionListItemContract source)
        {
            if (source.Amount < 0 || (source.Amount==0 &&source.Spent.Any()))
            {
                return true;
            }

            return false;
        }

        public static bool IsReceived(this AddressTransactionListItemContract source)
        {
            if (source.Amount > 0  || (source.Amount == 0 && source.Received.Any()))
            {
                return true;
            }

            return false;
        }
    }


}

