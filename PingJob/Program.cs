﻿using System.Configuration;
using System.Diagnostics;
using AzureRepositories;
using AzureRepositories.Log;
using AzureStorage.Tables;
using Common;
using Common.Log;
using Core.Settings;
using JobsCommon;
using Microsoft.Azure.WebJobs;

namespace PingJob
{
    class Program
    {
        static void Main()
        {
            var appSettings = CloudConfigurationLoader.ReadCloudConfiguration<AppSettings>();
#if DEBUG
            var settings = GeneralSettingsReader.ReadGeneralSettingsLocal<BaseSettings>();
#else
            var generalSettings = GeneralSettingsReader.ReadGeneralSettingsViaHttp<GeneralSettings>(ConfigurationManager.AppSettings["SettingsUrl"]);
            var settings = generalSettings.BcnExploler;
#endif

            appSettings.UpdateMainChainIndexerUrl = settings.ExplolerUrl.AddLastSymbolIfNotExists('/') +
                                                    "/mainchain/update/" + settings.Secret;

            var logToTable =
                new LogToTable(new AzureTableStorage<LogEntity>(settings.Db.LogsConnString, "LogPingFunctions",
                    null));
            var log = new LogToTableAndConsole(logToTable, new LogToConsole());
            var container = new DResolver();
            InitContainer(container, settings, appSettings, log);

            var config = new JobHostConfiguration
            {
                JobActivator = container,
                StorageConnectionString = settings.Db.LogsConnString,
                DashboardConnectionString = settings.Db.LogsConnString,
                Tracing = { ConsoleLevel = TraceLevel.Error }
            };
            
            config.UseTimers();

            if (settings.Jobs.IsDebug)
            {
                config.UseDevelopmentSettings();
            }

            var host = new JobHost(config);
            host.RunAndBlock();
        }

        private static void InitContainer(DResolver container, BaseSettings settings, AppSettings appSettings, ILog log)
        {
            log.WriteInfo("InitContainer", "Program", null, null).Wait();
            container.IoC.Register<ILog>(log);

            container.IoC.Register(appSettings);
            container.IoC.Register(settings);

            container.IoC.RegisterSingleTone<PingFunctions>();
        }
    }
}
