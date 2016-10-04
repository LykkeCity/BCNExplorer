using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureRepositories.GrabBlockTask;
using AzureStorage.Tables;
using Common.Log;
using Core.Settings;

namespace AzureRepositories
{
    public static class AzureRepoFactories
    {
        public static GrabBlockCommandsRepository CreateGrabBlockTaskRepository(BaseSettings baseSettings, ILog log)
        {
            return new GrabBlockCommandsRepository(new AzureTableStorage<PendingGrabBlockCommandEntity>(baseSettings.Db.GrabConnString, "GrabBlockCmds", log),
                new AzureTableStorage<GrabBlockFailedResultEntity>(baseSettings.Db.GrabConnString, "GrabBlockFailedResults", log), 
                new AzureTableStorage<GrabBlockDoneResultEntity>(baseSettings.Db.GrabConnString, "GrabBlockDoneResults", log), baseSettings);
        }
    }
}
