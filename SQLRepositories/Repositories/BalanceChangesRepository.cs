using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.AssetBlockChanges;
using EntityFramework.BulkInsert.Extensions;
using SQLRepositories.Context;
using SQLRepositories.DbModels;

namespace SQLRepositories.Repositories
{
    public class BalanceChangesRepository:IBalanceChangesRepository
    {
        private readonly BcnExplolerFactory _bcnExplolerFactory;
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(initialCount: 1);

        public BalanceChangesRepository(BcnExplolerFactory bcnExplolerFactory)
        {
            _bcnExplolerFactory = bcnExplolerFactory;
        }

        public async Task AddAsync(string legacyAddress, params IBalanceChange[] balanceChanges)
        {
            try
            {
                await _lock.WaitAsync();
                using (var db = _bcnExplolerFactory.GetContext())
                {
                    var blockHashes = balanceChanges.Select(p => p.BlockHash).Distinct();
                    var existedParsedAddressBlocks = await db.ParsedAddressBlockEntities
                        .Where(p => p.Address == legacyAddress && blockHashes.Contains(p.BlockHash))
                        .ToListAsync();

                    //not to add alredy parsed changes
                    var entities = balanceChanges.Select(BalanceChangeEntity.Create)
                        .Where(p => existedParsedAddressBlocks.All(epab => epab.Address != p.Address && epab.BlockHash != p.BlockHash));
                    
                    var postedParsedAddressBlocks = balanceChanges
                        .Select(p => new ParsedAddressBlockEntity
                        {
                            Address = p.Address,
                            BlockHash = p.BlockHash
                        }).Where(p => !existedParsedAddressBlocks.Contains(p, ParsedAddressBlockEntity.AddressBlockHashComparer))
                        .Distinct(ParsedAddressBlockEntity.AddressBlockHashComparer);
                    
                    db.BulkInsert(entities);
                    db.BulkInsert(postedParsedAddressBlocks);

                    await db.SaveChangesAsync();
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        //public async Task<BalanceSummary> GetSummaryAsync(params string[] assetIds)
        //{
        //    using (var db = _bcnExplolerFactory.GetContext())
        //    {
        //        var balances = await db.BalanceChanges.Where(p => assetIds.Contains(p.AssetId))
        //            .GroupBy(p => p.Address)
        //            .Select(p=> new
        //            {
        //                Address = p.Key,
        //                Balance = p.Sum(g => g.Change)
        //            }).ToListAsync();

        //        return new BalanceSummary
        //        {
        //            AssetId = assetIds.FirstOrDefault(),
        //            AddressSummaries = balances.Select(p => new BalanceSummary.BalanceAddressSummary
        //            {
        //                Address = p.Address,
        //                Balance = p.Balance
        //            })
        //        };
        //    }
        //}
    }
}
