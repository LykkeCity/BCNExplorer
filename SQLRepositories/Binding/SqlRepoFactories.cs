using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
