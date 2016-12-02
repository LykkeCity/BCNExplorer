using AssetCoinHoldersScanner.QueueHandlers;
using AssetCoinHoldersScanner.TimerFunctions;
using AzureRepositories;
using AzureRepositories.QueueReaders;
using AzureStorage.Queue;
using Common.IocContainer;
using Common.Log;
using Core.Settings;
using JobsCommon;

namespace AssetCoinHoldersScanner.Binders
{
    public static class AssetCoinHoldersFunctionsBinder
    {
        public static void BindAssetsCoinHoldersFunctions(this IoC ioc, BaseSettings baseSettings, ILog log)
        {
            ioc.Register<IParseBlockQueueReader>(AssetFunctionsFactories.CreateParseBlockCommandQueueReader(baseSettings, log));

            ioc.Register<ICoinholderIndexesQueueReader>(AssetFunctionsFactories.CreateCoinholderIndexesQueueReader(baseSettings, log));
            ioc.RegisterSingleTone<SendMonitorData>();
            ioc.RegisterSingleTone<ParseBlocksFunctions>();
            ioc.RegisterSingleTone<UpdateAssetIndexDataFunctions>();
            
            ioc.RegisterSingleTone<ParseBalanceChangesCommandQueueConsumer>();
        }
    }

    public static class AssetFunctionsFactories
    {

        public static ParseBlockQueueReader CreateParseBlockCommandQueueReader(BaseSettings baseSettings, ILog log)
        {
            var updateAssetDataQueue = new AzureQueueExt(baseSettings.Db.AssetsConnString, JobsQueueNames.AssetChangesParseBlockCommands);
            return new ParseBlockQueueReader(updateAssetDataQueue, "ParseBlockQueueReader", 5 * 1000, log);
        }

        public static CoinholderIndexesQueueReader CreateCoinholderIndexesQueueReader(BaseSettings baseSettings, ILog log)
        {
            var updateAssetDataQueue = new AzureQueueExt(baseSettings.Db.AssetsConnString, JobsQueueNames.AssetCoinhodersIndexesCommands);
            return new CoinholderIndexesQueueReader(updateAssetDataQueue, "ParseBlockQueueReader", 5 * 1000, log);
        }
    }
}
