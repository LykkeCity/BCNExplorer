using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Core.AssetBlockChanges;
using SQLRepositories.Context;
using SQLRepositories.DbModels;

namespace SQLRepositories.Repositories
{
    public class BlockRepository:IBlockRepository
    {
        private readonly BcnExplolerFactory _bcnExplolerFactory;

        public BlockRepository(BcnExplolerFactory bcnExplolerFactory)
        {
            _bcnExplolerFactory = bcnExplolerFactory;
        }

        public async Task AddAsync(IBlock[] blocks)
        {
            using (var db = _bcnExplolerFactory.GetContext())
            {
                var existed = await db.Blocks.ToListAsync();
                var posted =
                    blocks.Where(p => p != null).Select(BlockEntity.Create).Distinct(BlockEntity.HashComparer);
                //Do not add existed in db 
                var entitiesToAdd =
                    posted.Where(p => !existed.Contains(p, BlockEntity.HashComparer));

                db.Blocks.AddRange(entitiesToAdd);
                await db.SaveChangesAsync();
            }
        }
    }
}
