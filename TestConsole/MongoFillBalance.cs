using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.IocContainer;
using Core.AssetBlockChanges.Mongo;
using SQLRepositories.Context;
using SQLRepositories.DbModels;

namespace TestConsole
{
    public class MongoFillBalance
    {
        public static async Task Run(IoC ioc)
        {
            var balanceChangesRepo = ioc.GetObject<IAssetBalanceChangesRepository>();

            var contextFactory = ioc.GetObject<BcnExplolerFactory>();

            IEnumerable<AddressEntity> addr;
            using (var db = contextFactory.GetContext())
            {
                addr = db.Addresses.Where(p => !p.ParsedAddressBlockEntities.Any()).ToList();
                //addr = db.Addresses.Where(p => p.ColoredAddress== "akYc7BCwLpf1JWTnQdj8WN94Gajokn8MEhT").ToList();
            }
        }
    }

    



}
