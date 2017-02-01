using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureRepositories;
using AzureRepositories.BalanceReport;
using AzureRepositories.Binders;
using AzureRepositories.Log;
using AzureStorage.Tables;
using BalanceReporting.Binders;
using BalanceReporting.QueueHandlers;
using Common;
using Common.Log;
using Core.Settings;
using JobsCommon;
using Microsoft.Azure.WebJobs;
using Providers;
using Providers.Binders;
using Services.Binders;

namespace BalanceReporting
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
                    new LogToTable(new AzureTableStorage<LogEntity>(settings.Db.LogsConnString, "BalanceReporting",
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

                //TEST
                container.IoC.GetObject<SendBalanceReportCommandQueryProducer>()
                    .CreaseSendBalanceReportCommandAsync("netsky@bk.ru", "Volkov Andrey", new[] { "anMUe3LgGapNHxKsGxmtbsPpNeC33sa7y9a", "anJBX5sKFK4vnbywKWE2NQa9xrvLJEqRAB2" }, new DateTime(2016,12,31))
                    .Wait();
                //Test

                container.IoC.CreateInstance<BalanceReportQueueConsumer>().Start();

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

            settings.DisablePersistentCacheMainChain = true;
            container.IoC.Register(settings);
            container.IoC.BindAzureRepositories(settings, log);
            container.IoC.BindServices(settings, log);
            container.IoC.BindBalanceReportingFunctions(settings, log);
        }
    }
}
