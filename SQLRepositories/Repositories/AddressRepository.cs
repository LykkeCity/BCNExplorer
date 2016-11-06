using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Core.AssetBlockChanges;
using SQLRepositories.Context;
using SQLRepositories.DbModels;

namespace SQLRepositories.Repositories
{
    public class AddressRepository:IAddressRepository
    {
        private readonly BcnExplolerFactory _bcnExplolerFactory;

        public AddressRepository(BcnExplolerFactory bcnExplolerFactory)
        {
            _bcnExplolerFactory = bcnExplolerFactory;
        }

        public async Task AddAsync(IAddress[] addresses)
        {
            using (var db = _bcnExplolerFactory.GetContext())
            {
                var postedHashes = addresses.Select(p => p.LegacyAddress);
                var existed = await db.Addresses.Where(p => postedHashes.Contains(p.LegacyAddress)).ToListAsync();
                var posted =
                    addresses.Where(p => p != null).Select(AddressEntity.Create).Distinct(AddressEntity.LegacyAddressComparer);
                //Do not add existed in db 
                var entitiesToAdd =
                    posted.Where(p => !existed.Contains(p, AddressEntity.LegacyAddressComparer));

                db.Addresses.AddRange(entitiesToAdd);
                await db.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<IAddress>> GetAllAsync()
        {
            using (var db = _bcnExplolerFactory.GetContext())
            {
                return await db.Addresses.ToListAsync();
            }
        }
    }
}
