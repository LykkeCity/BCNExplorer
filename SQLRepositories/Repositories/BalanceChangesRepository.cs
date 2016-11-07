using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Core.AssetBlockChanges;
using SQLRepositories.Context;
using SQLRepositories.DbModels;

namespace SQLRepositories.Repositories
{
    public class BalanceChangesRepository:IBalanceChangesRepository
    {
        private readonly BcnExplolerFactory _bcnExplolerFactory;

        public BalanceChangesRepository(BcnExplolerFactory bcnExplolerFactory)
        {
            _bcnExplolerFactory = bcnExplolerFactory;
        }

        public async Task AddAsync(string legacyAddress, params IBalanceChange[] balanceChanges)
        {
            using (var db = _bcnExplolerFactory.GetContext())
            {
                var blockHashes = balanceChanges.Select(p => p.BlockHash);
                var existedParsedAddressBlocks = await db.ParsedAddressBlockEntities
                    .Where(p => p.Address == legacyAddress && blockHashes.Contains(p.BlockHash))
                    .ToListAsync();

                //not to add alredy parsed changes
                var entities = balanceChanges.Select(BalanceChangeEntity.Create)
                    .Where(p=> existedParsedAddressBlocks.All(epab => epab.Address != p.Address && epab.BlockHash!= p.BlockHash));

                db.BalanceChanges.AddRange(entities);


                var postedParsedAddressBlocks = balanceChanges
                    .Select(p => new ParsedAddressBlockEntity
                    {
                        Address = p.Address,
                        BlockHash = p.BlockHash
                    }).Where(p=> !existedParsedAddressBlocks.Contains(p, ParsedAddressBlockEntity.AddressBlockHashComparer))
                    .Distinct(ParsedAddressBlockEntity.AddressBlockHashComparer);

                db.ParsedAddressBlockEntities.AddRange(postedParsedAddressBlocks);

                await db.SaveChangesAsync();
            }
        }
    }
}
