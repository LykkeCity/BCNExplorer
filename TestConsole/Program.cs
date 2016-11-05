using AzureRepositories;
using AzureRepositories.Binders;
using Common.IocContainer;
using Common.Log;
using Core.Settings;
using JobsCommon;
using Providers;
using SQLRepositories.Binding;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = GeneralSettingsReader.ReadGeneralSettings<BaseSettings>(JobsConnectionStringSettings.ConnectionString);
     
            var container = new IoC();
            InitContainer(container, settings, new LogToConsole());

            //TestGettingChainChanges.Run(container);
            GetAddressesWithColoredAssets.Run(container);

        }
        
        private static void InitContainer(IoC container, BaseSettings settings, ILog log)
        {
            container.Register<ILog>(log);

            container.BindProviders(settings, log);
            container.Register(settings);
            container.BindAzureRepositories(settings, log);
            container.BindSqlRepos(settings, log);
        }

    }
}
