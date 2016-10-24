using System;
using System.Diagnostics;
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
using Providers;

namespace AssetCoinHolders
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
        }
    }
}
