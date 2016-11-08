using AzureRepositories.Monitoring;
using AzureStorage.Tables;
using Common.IocContainer;
using Common.Log;
using Core.Asset;
using Core.Monitoring;
using Core.Settings;

namespace AzureRepositories.Binders
{
    public static class AzureRepositoriesBinder
    {
        public static void BindAzureRepositories(this IoC ioc, BaseSettings baseSettings, ILog log)
        {
            ioc.Register<IAssetDefinitionRepository>(AzureRepoFactories.CreateAssetDefinitionsRepository(baseSettings, log));
            ioc.Register<IAssetDefinitionParsedBlockRepository>(AzureRepoFactories.CreateAssetParsedBlockRepository(baseSettings, log));

            ioc.Register(AzureRepoFactories.CreateUpdateAssetDataCommandProducer(baseSettings, log));
            ioc.Register(AzureRepoFactories.CreateAssetDefinitionParseBlockCommandProducer(baseSettings, log));
            ioc.Register(AzureRepoFactories.CreateAssetChangesParseBlockCommandProducer(baseSettings, log));

            ioc.Register<IServiceMonitoringRepository>(
                 new ServiceMonitoringRepository(
                     new AzureTableStorage<MonitoringRecordEntity>(baseSettings.Db.SharedStorageConnString, "Monitoring", log)));

        }
    }
}
