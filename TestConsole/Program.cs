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
using NBitcoin;
using NBitcoin.Indexer;
using Providers;
using Providers.Helpers;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = GeneralSettingsReader.ReadGeneralSettings<BaseSettings>(JobsConnectionStringSettings.ConnectionString);
     
            var container = new IoC();
            InitContainer(container, settings, new LogToConsole());

            var indexerClient = container.GetObject<IndexerClient>();

            //var tr = indexerClient.GetTransaction(uint256.Parse("e7f7ee1a7c2e915b236f788c7230faf9cd06e95988996111396345fe9ac4bedd"));


            var ordBalances = indexerClient.GetOrderedBalance(new BitcoinColoredAddress("akBGHM9p2SN5NoqvUAqGaRdZmMFjF88MViR")).ToArray();

            var alTx = new List<ColoredChange>();

            foreach (var bl in ordBalances)
            {
                var deltaChanges = bl.GetColoredChanges(Network.Main);

                Console.WriteLine("TxId: {0} ", bl.TransactionId);
                Console.WriteLine();

                foreach (var coloredChange in deltaChanges)
                {
                    Console.WriteLine("AssetId {0}, Quantity {1}", coloredChange.AssetId, coloredChange.Quantity);
                    alTx.Add(coloredChange);
                }

                Console.WriteLine("-----------");


            }

            foreach (var assetGrouping in alTx.GroupBy(p=>p.AssetId))
            {
                Console.WriteLine("Asset {0} Sum {1}", assetGrouping.Key, assetGrouping.Sum(p=>p.Quantity));
            }


            Console.ReadLine();
        }

        private static void ImportAssets()
        {
            //var detailsUrls = GetDetailsUrls().Result;

            //var res = GetDefUrls(detailsUrls).Result;

            //var defUrls = res.Ok.ToList();
            //var failedUrls = res.Failed;
            //while (failedUrls.Any())
            //{
            //    var t = GetDefUrls(failedUrls).Result;
            //    defUrls.AddRange(t.Ok);

            //    failedUrls = t.Failed;
            //}

            //var cmdProducer = container.GetObject<AssetDataCommandProducer>();
            //cmdProducer.CreateUpdateAssetDataCommand(defUrls.ToArray()).Wait();

            //Console.WriteLine("All done");
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
