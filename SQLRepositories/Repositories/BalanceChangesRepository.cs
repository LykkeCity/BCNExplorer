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

        public async Task AddAsync(params IBalanceChange[] balanceChanges)
        {
            using (var db = _bcnExplolerFactory.GetContext())
            {
                var entities = balanceChanges.Select(BalanceChangeEntity.Create);

                db.BalanceChanges.AddRange(entities);

                await db.SaveChangesAsync();
            }
        }
    }
}
