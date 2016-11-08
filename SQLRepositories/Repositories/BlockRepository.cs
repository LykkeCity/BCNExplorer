using System;
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
    public class BlockRepository:IBlockRepository
    {
        private readonly BcnExplolerFactory _bcnExplolerFactory;
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(initialCount: 1);

        public BlockRepository(BcnExplolerFactory bcnExplolerFactory)
        {
            _bcnExplolerFactory = bcnExplolerFactory;
        }

        public async Task AddAsync(IBlock[] blocks)
        {
            try
            {
                await _lock.WaitAsync();
                using (var db = _bcnExplolerFactory.GetContext())
                {
                    var posted = blocks.Where(p => p != null)
                        .Select(BlockEntity.Create)
                        .Distinct(BlockEntity.HashComparer)
                        .ToList();

                    var postedHashes = posted.Select(p => p.Hash).ToList();
                    var existed = await db.Blocks.Where(p => postedHashes.Contains(p.Hash)).ToListAsync();

                    //Do not add existed in db 
                    var entitiesToAdd =
                        posted.Where(p => !existed.Contains(p, BlockEntity.HashComparer))
                        .ToList();

                    db.BulkInsert(entitiesToAdd);
                    await db.SaveChangesAsync();
                }
            }
            finally
            {
                _lock.Release();
            }

        }
    }
}
