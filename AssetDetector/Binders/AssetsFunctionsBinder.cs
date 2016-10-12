using AssetScanner.QueueHandlers;
using AssetScanner.TimerFunctions;
using AzureRepositories;
using AzureRepositories.Asset;
using AzureStorage.Queue;
using Common.IocContainer;
using Common.Log;
using Core.Asset;
using Core.Settings;
using Providers.BlockChainReader;

namespace AssetScanner.Binders
{
    public static class AssetsFunctionsBinder
    {
        public static void BindAssetsFunctions(this IoC ioc, BaseSettings baseSettings, ILog log)
        {
            ioc.RegisterPerCall<ParseBlocksFunctions>();
            ioc.RegisterPerCall<UpdateAssetDataFunctions>();

            ioc.Register<IAssetDataQueueReader>(AssetFunctionsFactories.CreateUpdateAssetDataCommandQueueReader(baseSettings, log));
            ioc.Register<IParseBlockQueueReader>(AssetFunctionsFactories.CreateUpdateParseBlockCommandQueueReader(baseSettings, log));
            ioc.RegisterPerCall<UpdateAssetDataCommandQueueConsumer>();
            ioc.RegisterPerCall<ParseBlockCommandQueueConsumer>();
        }
    }

    public static class AssetFunctionsFactories
    {
        public static AssetDataQueueReader CreateUpdateAssetDataCommandQueueReader(BaseSettings baseSettings, ILog log)
        {
            var updateAssetDataQueue = new AzureQueueExt(baseSettings.Db.AssetsConnString, JobsQueueNames.UpdateAssetDataCommands);
            return new AssetDataQueueReader(updateAssetDataQueue, "UpdateAssetDataQueueReader", 5 * 1000, log);
        }

        public static ParseBlockQueueReader CreateUpdateParseBlockCommandQueueReader(BaseSettings baseSettings, ILog log)
        {
            var updateAssetDataQueue = new AzureQueueExt(baseSettings.Db.AssetsConnString, JobsQueueNames.ParseBlockCommands);
            return new ParseBlockQueueReader(updateAssetDataQueue, "ParseBlockQueueReader", 5 * 1000, log);
        }
        
    }
}
