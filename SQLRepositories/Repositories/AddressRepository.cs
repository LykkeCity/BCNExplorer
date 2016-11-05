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

        public async Task AddAddressesAsync(IAddress[] addresses)
        {
            using (var db = _bcnExplolerFactory.GetContext())
            {
                var exisedAddresses = await db.Addresses.ToListAsync();
                var postedAddresses =
                    addresses.Where(p => p != null).Select(AddressEntity.Create).Distinct(AddressEntity.LegacyAddressComparer);
                //Do not add existed in db addresses
                var addresesToAdd =
                    postedAddresses.Where(p => !exisedAddresses.Contains(p, AddressEntity.LegacyAddressComparer));

                db.Addresses.AddRange(addresesToAdd);
                await db.SaveChangesAsync();
            }
        }
    }
}
