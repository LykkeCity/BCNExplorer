using AzureRepositories.Monitoring;
using AzureStorage.Tables;
using Common.IocContainer;
using Common.Log;
using Core.Asset;
using Core.AssetBlockChanges.Mongo;
using Core.Monitoring;
using Core.Settings;
using Core.TransactionCache;

namespace AzureRepositories.Binders
{
    public static class AzureRepositoriesBinder
    {
        public static void BindAzureRepositories(this IoC ioc, BaseSettings baseSettings, ILog log)
        {
            ioc.Register<IAssetDefinitionRepository>(AzureRepoFactories.CreateAssetDefinitionsRepository(baseSettings, log));
            ioc.Register<IAssetDefinitionParsedBlockRepository>(AzureRepoFactories.CreateAssetParsedBlockRepository(baseSettings, log));
            ioc.Register<IAssetCoinholdersIndexRepository>(AzureRepoFactories.CreateAssetCoinholdersIndexRepository(baseSettings, log));
            ioc.Register<IAssetScoreRepository>(AzureRepoFactories.CreateAssetScoreRepository(baseSettings, log));
            ioc.Register<ITransactionCacheStatusRepository>(AzureRepoFactories.CreateTransactionCacheStatusRepository(baseSettings, log));
            ioc.Register<ITransactionCacheItemRepository>(AzureRepoFactories.CreateTransactionCacheItemRepository(baseSettings, log));
            ioc.Register<IAssetImageRepository>(AzureRepoFactories.CreateAssetImageRepository(baseSettings, log));
            
            ioc.Register(AzureRepoFactories.CreateAssetDataCommandProducer(baseSettings, log));
            ioc.Register(AzureRepoFactories.CreateAssetImageCommandProducer(baseSettings, log));
            
            ioc.Register(AzureRepoFactories.CreateAssetDefinitionParseBlockCommandProducer(baseSettings, log));
            ioc.Register(AzureRepoFactories.CreateAssetChangesParseBlockCommandProducer(baseSettings, log));
            ioc.Register(AzureRepoFactories.CreateAssetCoinholdersIndexesCommandProducer(baseSettings, log));
            ioc.Register(AzureRepoFactories.CreateSendBalanceReportCommandQueryProducer(baseSettings, log));

            ioc.Register<IAssetBalanceChangesRepository>(AzureRepoFactories.CreateAssetBalanceChangesRepository(baseSettings, log));
            
            ioc.Register<IServiceMonitoringRepository>(
                 new ServiceMonitoringRepository(
                     new AzureTableStorage<MonitoringRecordEntity>(baseSettings.Db.SharedStorageConnString, "Monitoring", log)));

        }
    }
}
