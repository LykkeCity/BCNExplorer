using System;
using System.Diagnostics;
using AssetDefinitionScanner.Binders;
using AssetDefinitionScanner.QueueHandlers;
using AssetDefinitionScanner.TimerFunctions;
using AzureRepositories;
using AzureRepositories.Binders;
using AzureRepositories.Log;
using AzureStorage.Tables;
using Common;
using Common.Log;
using Core.Settings;
using JobsCommon;
using Microsoft.Azure.WebJobs;
using Providers;
using Providers.Binders;
using Services.Binders;

namespace AssetDefinitionScanner
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

                var assetDefinitonCommandConsumer = container.IoC.CreateInstance<AssetDataCommandQueueConsumer>();
                assetDefinitonCommandConsumer.Start();

                var imageCommandConsumer = container.IoC.CreateInstance<AssetImagesCommandQueueConsumer>();
                imageCommandConsumer.Start();

                var parseBlockCommandQueueConsumer = container.IoC.CreateInstance<ParseBlockCommandQueueConsumer>();
                parseBlockCommandQueueConsumer.Start();

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
                log?.WriteFatalError("Program", "Main - App Start", null, e).Wait();
            }

            Console.ReadLine();
        }

        private static void InitContainer(DResolver container, BaseSettings settings, ILog log)
        {
            log.WriteInfo("InitContainer", "Program", null, null).Wait();
            container.IoC.Register<ILog>(log);

            container.IoC.RegisterSingleTone<SendMonitorData>();

            container.IoC.BindProviders(settings, log);
            container.IoC.Register(settings);
            container.IoC.BindAzureRepositories(settings, log);
            container.IoC.BindAssetsFunctions(settings, log);
            container.IoC.BindServices(settings, log);
        }
    }
}
