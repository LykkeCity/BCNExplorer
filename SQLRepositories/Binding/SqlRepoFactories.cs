using Common.Log;
using Core.Settings;
using SQLRepositories.Context;
using SQLRepositories.Repositories;

namespace SQLRepositories.Binding
{
    public static class SqlRepoFactories
    {
        public static BcnExplolerDataContext GetBcnExplolerDataContext(BaseSettings baseSettings, ILog log)
        {
            return new BcnExplolerDataContext(baseSettings.Db.SqlConnString);
        }

        public static AddressRepository GetAddressRepository(BaseSettings baseSettings, ILog log, BcnExplolerFactory bcnExplolerFactory)
        {
            return new AddressRepository(bcnExplolerFactory);
        }

        public static TransactionRepository GetTransactionRepository(BaseSettings baseSettings, ILog log, BcnExplolerFactory bcnExplolerFactory)
        {
            return new TransactionRepository(bcnExplolerFactory);
        }

        public static BlockRepository GetBlockRepository(BaseSettings baseSettings, ILog log, BcnExplolerFactory bcnExplolerFactory)
        {
            return new BlockRepository(bcnExplolerFactory);
        }

        public static BalanceChangesRepository GetBalanceChangesRepository(BaseSettings baseSettings, ILog log, BcnExplolerFactory bcnExplolerFactory)
        {
            return new BalanceChangesRepository(bcnExplolerFactory);
        }
    }
}
