using AzureRepositories.Grab;
using AzureStorage.Tables;
using Common.Log;
using Core.Settings;

namespace AzureRepositories
{
    public static class AzureRepoFactories
    {
        public static GrabBlockDirectorBase CreateGrabAssetsRepository(BaseSettings baseSettings, ILog log)
        {
            return new GrabNewAssets(new AzureTableStorage<GrabBlockCommandEntity>(baseSettings.Db.GrabConnString, "GrabTransactionCmds", log),
                new AzureTableStorage<GrabBlockFailedResultEntity>(baseSettings.Db.GrabConnString, "GrabTransactionFailedResults", log), 
                new AzureTableStorage<GrabBlockDoneResultEntity>(baseSettings.Db.GrabConnString, "GrabTransactionDoneResults", log), baseSettings.Jobs.MaxGrabTransactionAttemptCount);
        }
    }
}
