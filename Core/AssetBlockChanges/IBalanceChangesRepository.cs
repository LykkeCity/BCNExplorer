﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AssetBlockChanges
{
    public interface IBalanceChange
    {
        long Id { get; }
        string AssetId { get; }
        double Change { get; }
        string TransactionHash { get; }
        string Address { get; }
        string BlockHash { get; }
    }

    public class BalanceSummary
    {
        public string AssetId { get; set; }
        public IEnumerable<BalanceAddressSummary> AddressSummaries { get; set; }

        public class BalanceAddressSummary
        {
            public string Address { get; set; }
            public double Balance { get; set; }
        }
    }

    public class BalanceChange: IBalanceChange
    {
        public long Id { get;  }
        public string AssetId { get; set; }
        public double Change { get; set; }
        public string TransactionHash { get; set; }
        public string Address { get; set; }
        public string BlockHash { get; set; }
    }

    public interface IBalanceChangesRepository
    {
        Task AddAsync(string legacyAddress, params IBalanceChange[] balanceChanges);
        Task<BalanceSummary> GetSummaryAsync(params string[] assetIds);
    }
}