﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.AddressService;
using Providers.Providers.Ninja;

namespace Services.Address
{
    public class AddressBalance:IAddressBalance
    {
        public string AddressId { get; set; }
        public IEnumerable<IAddressTransaction> AllTransactionIds { get; set; }
        public IEnumerable<IAddressTransaction> SendTransactionIds { get; set; }
        public IEnumerable<IAddressTransaction> ReceivedTransactionIds { get; set; }
        public int TotalTransactions { get; set; }
        public string UncoloredAddress { get; set; }
        public string ColoredAddress { get; set; }
        public double Balance { get; set; }
        public double UnconfirmedBalanceDelta { get; set; }
        public IEnumerable<IColoredBalance> ColoredBalances { get; set; }

        public AddressBalance()
        {
            AllTransactionIds = Enumerable.Empty<IAddressTransaction>();
            SendTransactionIds = Enumerable.Empty<IAddressTransaction>();
            ReceivedTransactionIds = Enumerable.Empty<IAddressTransaction>();

            ColoredBalances = Enumerable.Empty<IColoredBalance>();
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

        public async Task<IAddressBalance> GetBalanceAsync(string id)
        {
            var ninjaAddress = new Lazy<AddressBalance>(() => new AddressBalance());
            var fillMainInfoTask = _ninjaAddressProvider.GetAliases(id).ContinueWith(task =>
            {
                if (task.Result != null)
                {
                    ninjaAddress.Value.AddressId = id;
                    ninjaAddress.Value.ColoredAddress = task.Result.ColoredAddress;
                    ninjaAddress.Value.UncoloredAddress = task.Result.UncoloredAddress;
                }
            });

            var fillTransactionsTask = _ninjaAddressProvider.GetTransactionsForAddressAsync(id).ContinueWith(task =>
            {
                if (task.Result != null)
                {
                    ninjaAddress.Value.AllTransactionIds = task.Result.AllTransactions.Select(AddressTransaction.Create);
                    ninjaAddress.Value.SendTransactionIds = task.Result.SendTransactions.Select(AddressTransaction.Create);
                    ninjaAddress.Value.ReceivedTransactionIds = task.Result.ReceivedTransactions.Select(AddressTransaction.Create);
                }
            });

            var fillSummaryTask = _ninjaAddressProvider.GetAddressBalanceAsync(id).ContinueWith(task =>
            {
                if (task.Result != null)
                {
                    ninjaAddress.Value.Balance = task.Result.Confirmed.Balance;
                    ninjaAddress.Value.TotalTransactions = task.Result.Confirmed.TotalTransactions;

                    ninjaAddress.Value.UnconfirmedBalanceDelta = task.Result.Unconfirmed.Balance;
                    var unconfirmedAssets = task.Result.Unconfirmed.Assets;

                    ninjaAddress.Value.ColoredBalances = (task.Result.Confirmed.Assets)
                        .Select(a =>
                        {
                            var coloredBalance = new ColoredBalance
                            {
                                AssetId = a.AssetId,
                                Quantity = a.Quantity
                            };
                            var unconfirmedAsset = unconfirmedAssets.FirstOrDefault(p => p.AssetId == coloredBalance.AssetId);
                            if (unconfirmedAsset != null)
                            {
                                coloredBalance.UnconfirmedQuantityDelta = unconfirmedAsset.Quantity;
                            }
                            return coloredBalance;
                        });
                }
            });

            await Task.WhenAll(fillMainInfoTask, fillTransactionsTask, fillSummaryTask);

            return ninjaAddress.IsValueCreated ? ninjaAddress.Value : null;
        }
    }
}
