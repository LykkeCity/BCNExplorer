using AssetIndexer.TimerFunctions;
using AzureRepositories;
using AzureRepositories.QueueReaders;
using AzureStorage.Queue;
using Common.IocContainer;
using Common.Log;
using Core.Settings;

namespace AssetIndexer.Binders
{
    public static class AssetIndexerFunctionsBinder
    {
        public static void BindAssetsIndexerFunctions(this IoC ioc, BaseSettings baseSettings, ILog log)
        {
            ioc.Register<ICoinholderIndexesQueueReader>(AssetFunctionsFactories.CreateCoinholderIndexesQueueReader(baseSettings, log));
            ioc.RegisterSingleTone<AssetIndexFunctions>();
            ioc.RegisterSingleTone<AssetScoreFunctions>();
        }
    }

    public static class AssetFunctionsFactories
    {
        public static CoinholderIndexesQueueReader CreateCoinholderIndexesQueueReader(BaseSettings baseSettings, ILog log)
        {
            var updateAssetDataQueue = new AzureQueueExt(baseSettings.Db.AssetsConnString, JobsQueueNames.AssetCoinhodersIndexesCommands);
            return new CoinholderIndexesQueueReader(updateAssetDataQueue, "ParseBlockQueueReader", 5 * 1000, log);
        }
    }
}
