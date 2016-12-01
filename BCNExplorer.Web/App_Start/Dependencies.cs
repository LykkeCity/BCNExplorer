using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;
using AzureRepositories;
using AzureRepositories.Binders;
using AzureRepositories.Log;
using AzureStorage.Tables;
using Common;
using Common.IocContainer;
using Common.Log;
using Common.Validation;
using Core.Settings;
using Providers;
using Services.Binders;

namespace BCNExplorer.Web.App_Start
{
    public class Dependencies
    {
        public static class WebSiteSettings
        {

            public static string ConnectionString => !string.IsNullOrEmpty(ConfigurationManager.AppSettings["ConnectionString"]) ? ConfigurationManager.AppSettings["ConnectionString"]: "UseDevelopmentStorage=true";
        }

        public static IDependencyResolver CreateDepencencyResolver()
        {
            var dr = new MyDependencyResolver();
            
            var settings = GeneralSettingsReader.ReadGeneralSettings<BaseSettings>(WebSiteSettings.ConnectionString);
            settings.NinjaUrl = settings.NinjaUrl.AddLastSymbolIfNotExists('/');

            GeneralSettingsValidator.Validate(settings);

            var log = new LogToTable(new AzureTableStorage<LogEntity>(settings.Db.LogsConnString, "BCNExplorer.WEB", null));

            dr.IoC.Register<ILog>(log);
            dr.IoC.Register(settings);
            
            dr.IoC.BindAzureRepositories(settings, log);
            dr.IoC.BindProviders(settings, log);
            dr.IoC.BindServices(settings, log);

            return dr;
        }


        public class MyDependencyResolver : IDependencyResolver
        {

            public readonly IoC IoC = new IoC();

            public void RegisterSingleTone<T, TI>() where TI : T
            {
                IoC.RegisterSingleTone<T, TI>();
            }

            public void RegisterSingleTone<T, TI>(TI instance) where TI : T
            {
                IoC.Register<T>(instance);
            }

            public T GetType<T>() where T : class
            {
                var result = IoC.GetObject<T>();
                return result;
            }

            public object GetService(Type serviceType)
            {
                var result = IoC.CreateInstance(serviceType);

                return result;
            }

            private readonly object[] _nullData = new object[0];
            public IEnumerable<object> GetServices(Type serviceType)
            {
                var result = IoC.CreateInstance(serviceType);
                return result == null ? _nullData : new[] { result };
            }
        }
    }
}