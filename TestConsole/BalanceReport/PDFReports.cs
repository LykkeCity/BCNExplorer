using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureRepositories.BalanceReport;
using BalanceReporting.QueueHandlers;
using Common;
using Common.IocContainer;
using Providers.Providers.Lykke.API;
using Services.MainChain;

namespace TestConsole.BalanceReport
{
    public static class PDFReports
    {
        public static async Task Run(IoC container)
        {
            #region WarmUP

            var mc = container.GetObject<MainChainService>();
            var cachedAssets = container.GetObject<CachedDataDictionary<string, IAsset>>();
            var cachedAssetPairs = container.GetObject<CachedDataDictionary<string, IAssetPair>>();

            var chain = await mc.GetMainChainAsync();
            await cachedAssetPairs.GetDictionaryAsync();
            await cachedAssets.GetDictionaryAsync();
            
            #endregion


            var producer=container.GetObject<SendBalanceReportCommandQueryProducer>();
            var cons = container.GetObject<BalanceReportQueueConsumer>();
            Console.WriteLine("Reading File");
            var users = File.ReadAllText("./users.json").DeserializeJson<UserModel[]>();

            var counter = users.Length;
            var semaphore = new SemaphoreSlim(50);
            var tasks = new List<Task>();
            foreach (var userModel in users)
            {
                await semaphore.WaitAsync();

                tasks.Add(cons.SendBalanceReport(SendBalanceReportCommand.Create("netsky@bk.ru", userModel.FullName,
                    userModel.Addresses, new DateTime(2016, 12, 31, 23, 59, 59)))
                    .ContinueWith(p =>
                    {
                        semaphore.Release(1);
                        Console.WriteLine(counter);
                        counter--;
                    }));
            }

            await Task.WhenAll(tasks);

            Console.WriteLine("All done");
            Console.ReadLine();
        }

        public class UserModel
        {
            public string Email { get; set; }

            public string FullName { get; set; }

            public string[] Addresses { get; set; }
        }


    }
}
