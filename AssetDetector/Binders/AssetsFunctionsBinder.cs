using AssetDefinitionScanner.QueueHandlers;
using AssetDefinitionScanner.TimerFunctions;
using AzureRepositories;
using AzureRepositories.Asset;
using AzureRepositories.AssetDefinition;
using AzureRepositories.QueueReaders;
using AzureStorage.Queue;
using Common.IocContainer;
using Common.Log;
using Core.Settings;
using JobsCommon;

namespace AssetDefinitionScanner.Binders
{
    public static class AssetsFunctionsBinder
    {
        public static void BindAssetsFunctions(this IoC ioc, BaseSettings baseSettings, ILog log)
        {
            ioc.RegisterPerCall<ParseBlocksFunctions>();
            ioc.RegisterPerCall<UpdateAssetDataFunctions>();

            ioc.Register<IAssetDataQueueReader>(AssetFunctionsFactories.CreateAssetDataCommandQueueReader(baseSettings, log));
            ioc.Register<IAssetImageQueueReader>(AssetFunctionsFactories.CreateAssetImageQueueReader(baseSettings, log));
            ioc.Register<IParseBlockQueueReader>(AssetFunctionsFactories.CreateUpdateParseBlockCommandQueueReader(baseSettings, log));
            ioc.RegisterPerCall<AssetDataCommandQueueConsumer>();
            ioc.RegisterPerCall<ParseBlockCommandQueueConsumer>();
        }
    }

    public static class AssetFunctionsFactories
    {
        public static AssetDataQueueReader CreateAssetDataCommandQueueReader(BaseSettings baseSettings, ILog log)
        {
            var updateAssetDataQueue = new AzureQueueExt(baseSettings.Db.AssetsConnString, JobsQueueNames.UpdateAssetDataCommands);
            return new AssetDataQueueReader(updateAssetDataQueue, "AssetDataQueueReader", 5 * 1000, log);
        }

        public static AssetImageQueueReader CreateAssetImageQueueReader(BaseSettings baseSettings, ILog log)
        {
            var updateAsseImageDataQueue = new AzureQueueExt(baseSettings.Db.AssetsConnString, JobsQueueNames.UpdateAssetImageCommands);
            return new AssetImageQueueReader(updateAsseImageDataQueue, "AssetImageQueueReader", 5 * 1000, log);
        }

        public static ParseBlockQueueReader CreateUpdateParseBlockCommandQueueReader(BaseSettings baseSettings, ILog log)
        {
            var updateAssetDataQueue = new AzureQueueExt(baseSettings.Db.AssetsConnString, JobsQueueNames.AssetDefinitionParseBlockCommands);
            return new ParseBlockQueueReader(updateAssetDataQueue, "ParseBlockQueueReader", 5 * 1000, log);
        }
        
    }
}
