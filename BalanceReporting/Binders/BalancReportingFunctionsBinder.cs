using AzureRepositories;
using AzureRepositories.QueueReaders;
using AzureStorage.Queue;
using BalanceReporting.QueueHandlers;
using BalanceReporting.TimerFunctions;
using Common.IocContainer;
using Common.Log;
using Core.Settings;

namespace BalanceReporting.Binders
{
    public static class BalancReportingFunctionsBinder
    {
        public static void BindBalanceReportingFunctions(this IoC ioc, BaseSettings baseSettings, ILog log)
        {
            ioc.RegisterSingleTone<SendMonitorData>();

            ioc.Register<IBalanceReportingQueueReader>(AssetFunctionsFactories.CreateBalanceReportingQueueReader(baseSettings, log));
            ioc.RegisterSingleTone<BalanceReportQueueConsumer>();
        }
    }

    public static class AssetFunctionsFactories
    {
        public static BalanceReportingQueueReader CreateBalanceReportingQueueReader(BaseSettings baseSettings, ILog log)
        {
            var updateAssetDataQueue = new AzureQueueExt(baseSettings.Db.SharedStorageConnString, JobsQueueNames.BalaceReporting);
            return new BalanceReportingQueueReader(updateAssetDataQueue, "BalanceReportQueueReader", 5 * 1000, log);
        }
    }
}
