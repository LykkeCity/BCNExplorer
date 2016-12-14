using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.IocContainer;
using Core.Asset;
using Core.AssetBlockChanges.Mongo;
using Core.Transaction;

namespace TestConsole.Coninholders
{
    public static class AssetCoinholdersToCsv
    {
        public static async Task Run(IoC container)
        {
            Console.WriteLine("AssetCoinholdersToCsv started");

            var filePath = "./AssetCoinholders.csv";

            var balanceChangesRepository = container.GetObject<IAssetBalanceChangesRepository>();
            var txService = container.GetObject<ITransactionService>();

            var strBuilder = new StringBuilder();
            strBuilder.AppendLine($"AssetIds|Top 20 coinholders|ChangedAtBlockHeights|CoinholdersCount|TotalQuantity|TotalTransactionsCount|LastMonthTxCount|LastTxDateTime|LastMonthTxVolume");
            var indexes = await container.GetObject<IAssetCoinholdersIndexRepository>().GetAllAsync();
            var counter = indexes.Count();
            foreach (var assetCoinholdersIndex in indexes)
            {
                Console.WriteLine(counter);
                counter--;
                var totalTx = await balanceChangesRepository.GetTransactionsAsync(assetCoinholdersIndex.AssetIds);
                var fromBLock = 438500;
                var lastMonthtx = await balanceChangesRepository.GetTransactionsAsync(assetCoinholdersIndex.AssetIds, fromBLock);
                var lastMonthBalance =
                    await
                        balanceChangesRepository.GetSummaryAsync(QueryOptions.Create().From(fromBLock),
                            assetCoinholdersIndex.AssetIds.ToArray());
                var lastMonthVolume = lastMonthBalance.AddressSummaries.Sum(p => p.Balance);

                var lastTx = await balanceChangesRepository.GetLatestTxAsync(assetCoinholdersIndex.AssetIds);
                var lastTxData = await txService.GetAsync(lastTx?.Hash);
                
                var arr = new []
                {
                    assetCoinholdersIndex.AssetIds.ToJson(),
                    assetCoinholdersIndex.CoinholdersCount.ToString(),
                    assetCoinholdersIndex.TotalQuantity.ToString(),
                    totalTx.Count().ToString(),
                    lastMonthtx.Count().ToString(),
                    lastTxData?.Block?.Time.ToString(),
                    lastMonthVolume.ToString()
                };
                var str = string.Join("|", arr);
                //Console.WriteLine(str);

                strBuilder.AppendLine(str);
            }
            File.Delete(filePath);
            File.WriteAllText(filePath, strBuilder.ToString());
            Console.WriteLine("AssetCoinholdersToCsv done");
            Console.ReadLine();
        }
    }
}
