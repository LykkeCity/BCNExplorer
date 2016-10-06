using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureRepositories.Asset;
using Common.IocContainer;
using Common.Log;
using Core.Asset;
using Core.GrabTransactionTask;
using Core.Settings;

namespace AzureRepositories.Binders
{
    public static class AzureRepositoriesBinder
    {
        public static void BindAzureRepositories(this IoC ioc, BaseSettings baseSettings, ILog log)
        {
            ioc.Register<IAssetRepository>(AzureRepoFactories.CreateAssetRepository(baseSettings, log));

            ioc.Register<UpdateAssetDataCommandProducer>(AzureRepoFactories.CreateUpdateAssetDataCommandProducer(baseSettings, log));
        }
    }
}
