using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AzureRepositories;
using AzureRepositories.Asset;
using AzureRepositories.Binders;
using Common.IocContainer;
using Common.Log;
using Core.Settings;
using JobsCommon;
using Providers;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = GeneralSettingsReader.ReadGeneralSettings<BaseSettings>(JobsConnectionStringSettings.ConnectionString);
     
            var container = new IoC();
            InitContainer(container, settings, new LogToConsole());
            var detailsUrls = GetDetailsUrls().Result;

            var res = GetDefUrls(detailsUrls).Result;

            var defUrls = res.Ok.ToList();
            var failedUrls = res.Failed;
            while (failedUrls.Any())
            {
                var t = GetDefUrls(failedUrls).Result;
                defUrls.AddRange(t.Ok);

                failedUrls = t.Failed;
            }
            
            var cmdProducer = container.GetObject<AssetDataCommandProducer>();
            cmdProducer.CreateUpdateAssetDataCommand(defUrls.ToArray()).Wait();


            Console.WriteLine("All done");

            Console.ReadLine();


        }

        private static void InitContainer(IoC container, BaseSettings settings, ILog log)
        {
            container.Register<ILog>(log);

            container.BindProviders(settings, log);
            container.Register(settings);
            container.BindAzureRepositories(settings, log);
        }


        public static async Task<IEnumerable<string>> GetDetailsUrls()
        {
            var config = Configuration.Default.WithDefaultLoader();
            var address = "https://www.coinprism.info/assets";
            var listDoc = await BrowsingContext.New(config).OpenAsync(address);
            var links = listDoc.QuerySelectorAll(".expand-column a");
            var defailsUrls = links.Select(m => "https://www.coinprism.info" + m.Attributes["href"].Value.ToString());

            return defailsUrls;
        }


        public static async Task<DefUrlResult> GetDefUrls(IEnumerable<string> defailsUrls)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var getDefUrlTasks = new List<Task>();
            var ok = new List<string>();
            var fail = new List<string>();
            foreach (var defailsUrl in defailsUrls)
            {
                var task = BrowsingContext.New(config).OpenAsync(defailsUrl).ContinueWith(detailsDoc =>
                {
                    try
                    {
                        var assetId = detailsDoc.Result.QuerySelectorAll(".expand-column").First().TextContent.Trim();
                        var assetDefUrl = detailsDoc.Result.QuerySelectorAll(".expand-column").Last().TextContent.Trim();

                        Console.WriteLine("Done " + assetDefUrl);
                        ok.Add(assetDefUrl);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed " + detailsDoc.Result.Url);
                        fail.Add(detailsDoc.Result.Url);
                    }

                });

                getDefUrlTasks.Add(task);

            }

            await Task.WhenAll(getDefUrlTasks);

            return new DefUrlResult
            {
              Ok  = ok,
              Failed = fail
            };
        }




        public class DefUrlResult
        {
            public IEnumerable<string> Ok { get; set; }
            
            public IEnumerable<string> Failed { get; set; } 
        }
    }
}
