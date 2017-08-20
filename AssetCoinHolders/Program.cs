using System;
using System.Configuration;
using System.Diagnostics;
using AssetCoinHoldersScanner.Binders;
using AssetCoinHoldersScanner.QueueHandlers;
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

namespace AssetCoinHoldersScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            LogToTableAndConsole log = null;
            try
            {
#if DEBUG
            var settings = GeneralSettingsReader.ReadGeneralSettingsLocal<BaseSettings>("../settings.json");
#else
                var generalSettings = GeneralSettingsReader.ReadGeneralSettingsViaHttp<GeneralSettings>(ConfigurationManager.AppSettings["SettingsUrl"]);
                var settings = generalSettings.BcnExploler;
#endif
                var logToTable =
                    new LogToTable(new AzureTableStorage<LogEntity>(settings.Db.LogsConnString, "LogAssetCoinHolders",
                        null));
                log = new LogToTableAndConsole(logToTable, new LogToConsole());

                var container = new DResolver();
                InitContainer(container, settings, log);
                
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

                var parseBlockCommandQueueConsumer = container.IoC.CreateInstance<ParseBalanceChangesCommandQueueConsumer>();
                parseBlockCommandQueueConsumer.Start();
                
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

            container.IoC.BindProviders(settings, log);

            settings.DisablePersistentCacheMainChain = true;
            container.IoC.Register(settings);

            container.IoC.BindAzureRepositories(settings, log);

            container.IoC.BindServices(settings, log);

            container.IoC.BindAssetsCoinHoldersFunctions(settings, log);
        }
    }
}
