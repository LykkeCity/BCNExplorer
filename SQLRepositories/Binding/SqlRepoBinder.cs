using Common.IocContainer;
using Common.Log;
using Core.AssetBlockChanges;
using Core.Settings;
using SQLRepositories.Context;
using SQLRepositories.Repositories;

namespace SQLRepositories.Binding
{
    public static class SqlRepoBinder
    {
        public static void BindSqlRepos(this IoC ioc, BaseSettings baseSettings, ILog log)
        {
            ioc.RegisterSingleTone<BcnExplolerFactory>();
            ioc.Register<IAddressRepository>(SqlRepoFactories.GetAddressRepository(baseSettings, log, ioc.GetObject<BcnExplolerFactory>()));
        }
    }
}
