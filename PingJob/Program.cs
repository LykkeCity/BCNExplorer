using System.Diagnostics;
using AzureRepositories;
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
            var settings = GeneralSettingsReader.ReadGeneralSettings<BaseSettings>(appSettings.ConnectionString);

            var container = new DResolver();
            InitContainer(container, settings, appSettings);

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

        private static void InitContainer(DResolver container, BaseSettings settings, AppSettings appSettings)
        {
            container.IoC.Register(appSettings);
            container.IoC.Register(settings);

            container.IoC.RegisterSingleTone<PingFunctions>();
        }
    }
}
