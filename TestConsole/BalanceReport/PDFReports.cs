using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureRepositories.BalanceReport;
using AzureStorage.Tables;
using BalanceReporting.QueueHandlers;
using Common;
using Common.IocContainer;
using Common.Log;
using Core.Settings;
using Microsoft.WindowsAzure.Storage.Table;
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

            var log = container.GetObject<ILog>();
            var baseSettings = container.GetObject<BaseSettings>();

            var producer=container.GetObject<SendBalanceReportCommandQueryProducer>();
            var cons = container.GetObject<BalanceReportQueueConsumer>();
            Console.WriteLine("Reading File");
            var users = File.ReadAllText("./users.json").DeserializeJson<UserModel[]>();

            var semaphore = new SemaphoreSlim(50);
            var tasks = new List<Task>();

            var okStorage = new AzureTableStorage<UserEntity>(baseSettings.Db.AssetsConnString, "UsersOK", log);
            var failStorage = new AzureTableStorage<UserEntity>(baseSettings.Db.AssetsConnString, "UsersFAIL", log);

            var parsed = (await okStorage.GetDataAsync()).Select(p=>p.Email);
            users = users.Where(p => !parsed.Contains(p.Email)).ToArray();
            var counter = users.Length;
            
            foreach (var userModel in users)
            {
                await semaphore.WaitAsync();

                tasks.Add(cons.SendBalanceReport(SendBalanceReportCommand.Create("netsky@bk.ru", userModel.FullName,
                    userModel.Addresses, new DateTime(2016, 12, 31, 23, 59, 59)))
                    .ContinueWith(async p =>
                    {
                        semaphore.Release(1);
                        if (p.Exception != null)
                        {
                            await failStorage.InsertAsync(UserEntity.Create(userModel.Email));
                        }
                        else
                        {
                            Console.WriteLine(counter);
                            counter--;
                            await okStorage.InsertAsync(UserEntity.Create(userModel.Email));
                        }
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

        public class UserEntity:TableEntity
        {
            public string Email { get; set; }
            
            public static UserEntity Create(string email)
            {
                return new UserEntity
                {
                    PartitionKey = "PK",
                    RowKey = email.ToBase64(),
                    Email = email
                };
            }
        }
    }
}
