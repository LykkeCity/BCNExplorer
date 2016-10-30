using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AssetCoinHoldersScanner.Binders;
using AssetCoinHoldersScanner.QueueHandlers;
using AssetCoinHoldersScanner.TimerFunctions;
using AzureRepositories;
using AzureRepositories.Binders;
using AzureRepositories.Log;
using AzureStorage.Tables;
using Common;
using Common.Log;
using Core.Settings;
using JobsCommon;
using JobsCommon.Binders;
using Microsoft.Azure.WebJobs;
using NBitcoin;
using NBitcoin.Indexer;
using Providers;
using Providers.Helpers;

namespace AssetCoinHoldersScanner
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
                
                //var parseBlockCommandQueueConsumer = container.IoC.CreateInstance<ParseBlockCommandQueueConsumer>();
                //parseBlockCommandQueueConsumer.Start();
                TestRetrieveChanges(container.IoC.GetObject<MainChainRepository>(), log,
                    container.IoC.GetObject<IndexerClient>()).Wait();
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
            log.WriteInfo("InitContainer", "Program", null, $"BaseSettings : {settings.ToJson()}").Wait();
            container.IoC.Register<ILog>(log);

            container.IoC.BindProviders(settings, log);
            container.IoC.Register(settings);
            container.IoC.BindAzureRepositories(settings, log);
            container.IoC.BindJobsCommon(settings, log);
            container.IoC.BindAssetsCoinHoldersFunctions(settings, log);
        }

        private static async Task TestRetrieveChanges(MainChainRepository mainChainRepository, ILog log, IndexerClient indexerClient)
        {
            var st = new Stopwatch();
            var mainchain = await mainChainRepository.GetMainChainAsync();
            var coloredAddresses = indexerClient.GetBlock(uint256.Parse("0000000000000000029559b0665cacb4470eda0696a69744263e82e7e4d0f27d")).GetAddresses(Network.Main);
            var checkTasks = new List<Task>();

            await log.WriteInfo("TestRetrieveChanges", "TestRetrieveChanges", st.Elapsed.ToString("g"), "Started");

            var semaphore = new SemaphoreSlim(100);
            foreach (var address in coloredAddresses)
            {
                var balanceId = BalanceIdHelper.Parse(address.ToString(), Network.Main);
                checkTasks.Add(indexerClient.GetConfirmedBalanceChangesAsync(balanceId, mainchain, semaphore));
            }

            await Task.WhenAll(checkTasks);

            st.Stop();
            
            await log.WriteInfo("TestRetrieveChanges", "TestRetrieveChanges", st.Elapsed.ToString("g"), "Finished");
        }
    }
}
