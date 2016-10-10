using System;
using System.Diagnostics;
using AssetScanner.Functions;
using AssetScanner.QueueHandlers;
using AzureRepositories;
using AzureRepositories.Binders;
using AzureRepositories.Log;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common;
using Common.Log;
using Core.Settings;
using JobsCommon;
using Microsoft.Azure.WebJobs;
using Providers;

namespace AssetScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            LogToTableAndConsole log = null;
            try
            {
                var settings =
                    GeneralSettingsReader.ReadGeneralSettings<BaseSettings>(
                        JobsConnectionStringSettings.ConnectionString);

                var logToTable =
                    new LogToTable(new AzureTableStorage<LogEntity>(settings.Db.LogsConnString, "LogAssetScanner",
                        null));
                log = new LogToTableAndConsole(logToTable, new LogToConsole());

                var container = new DResolver();
                InitContainer(container, settings, log);

                var emailQueueHandler = container.IoC.CreateInstance<UpdateAssetDataCommandQueueConsumer>();
                emailQueueHandler.Start();

                var config = new JobHostConfiguration
                {
                    JobActivator = container,
                    DashboardConnectionString = settings.Db.LogsConnString,
                    Tracing = { ConsoleLevel = TraceLevel.Error },
                    StorageConnectionString = settings.Db.AssetsConnString,
                    
                };

                config.UseTimers();

                if (settings.Jobs.IsDebug)
                {
                    config.UseDevelopmentSettings();
                }

                var host = new JobHost(config);
                host.RunAndBlock();
            }
            catch (Exception e)
            {
                log?.WriteFatalError("AssetScanner", "Main - App Start", null, e).Wait();
            }

            Console.ReadLine();
        }

        private static void InitContainer(DResolver container, BaseSettings settings, ILog log)
        {
            log.WriteInfo("InitContainer", "App start", null, $"BaseSettings : {settings.ToJson()}").Wait();
            container.IoC.Register<ILog>(log);

            container.IoC.BindProviders(settings, log);
            container.IoC.Register(settings);
            container.IoC.BindAssetsFunctions();
            container.IoC.BindAzureRepositories(settings, log);

            var updateAssetDataQueue = new AzureQueueExt(settings.Db.AssetsConnString, JobsQueueNames.UpdateAssetDataTasks);
            var updateAssetDataQueueReader = new QueueReader(updateAssetDataQueue, "UpdateAssetDataQueueReader", 5*1000, log);
            container.IoC.Register<IQueueReader> (updateAssetDataQueueReader);
            container.IoC.RegisterSingleTone<UpdateAssetDataCommandQueueConsumer>();
        }
    }
}
