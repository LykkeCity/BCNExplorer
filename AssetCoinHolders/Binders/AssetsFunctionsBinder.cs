using AzureRepositories;
using AzureRepositories.AssetDefinition;
using AzureStorage.Queue;
using Common.IocContainer;
using Common.Log;
using Core.Settings;

namespace AssetCoinHoldersScanner.Binders
{
    public static class AssetsFunctionsBinder
    {
        public static void BindAssetsFunctions(this IoC ioc, BaseSettings baseSettings, ILog log)
        {
            ioc.Register<IParseBlockQueueReader>(AssetFunctionsFactories.CreateUpdateParseBlockCommandQueueReader(baseSettings, log));
        }
    }

    public static class AssetFunctionsFactories
    {

        public static ParseBlockQueueReader CreateUpdateParseBlockCommandQueueReader(BaseSettings baseSettings, ILog log)
        {
            var updateAssetDataQueue = new AzureQueueExt(baseSettings.Db.AssetsConnString, JobsQueueNames.AssetChangesParseBlockCommands);
            return new ParseBlockQueueReader(updateAssetDataQueue, "ParseBlockQueueReader", 5 * 1000, log);
        }
        
    }
}
