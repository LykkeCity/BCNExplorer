using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureRepositories.AssetCoinHolders;
using BCNExplorer.Web.Models;
using Common;
using Common.IocContainer;
using Core.AddressService;
using Core.Asset;
using Core.AssetBlockChanges.Mongo;
using Core.Block;

namespace TestConsole
{
    public static class CheckBalance
    {
        public static async Task Run(IoC container)
        {
            var _assetProvider = container.GetObject<IAssetService>();
            var _blockService = container.GetObject<IBlockService>();
            var _addressProvider = container.GetObject<IAddressService>();
            var _balanceChangesRepository = container.GetObject<IAssetBalanceChangesRepository>();
            var id = "AWm6LaxuJgUQqJ372qeiUxXhxRWTXfpzog";
            var asset = await _assetProvider.GetAssetAsync(id);

            var result = AssetCoinholdersViewModel.Create(
                AssetViewModel.Create(asset),
                await _balanceChangesRepository.GetSummaryAsync(asset.AssetIds.ToArray()),
                 null, new Dictionary<string, double>(), Enumerable.Empty<BalanceBlock>(),
                await _blockService.GetLastBlockHeaderAsync(),
                null);
            var file = "./diff.txt";
            File.Delete(file);
            var counter = result.AddressSummaries.Count();
            foreach (var sum in result.AddressSummaries.OrderByDescending(p => p.Balance))
            {
                try
                {
                    Console.WriteLine(counter);
                    counter--;
                    var ninjablance = await _addressProvider.GetBalanceAsync(sum.Address);
                    var assetQuntity = ninjablance.ColoredBalances.Single(p => p.AssetId == id).Quantity;
                    var assetBalance = BitcoinUtils.CalculateColoredAssetQuantity(assetQuntity, asset.Divisibility);
                    if (assetBalance != sum.Balance)
                    {
                        var text = $" fail {sum.Address} {sum.Balance} {assetBalance}";
                        var currentColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(text);
                        Console.ForegroundColor = currentColor;
                        File.AppendAllLines(file, new[] { text });

                    }
                    else
                    {
                        Console.WriteLine($" ok {sum.Address} {sum.Balance}");
                    }
                }
                catch (Exception e)
                {

                    File.AppendAllLines(file, new[] { e.ToString() });
                }

            }

            Console.WriteLine("all done");

            Console.ReadLine();
            throw new NotImplementedException();
        }
    }
}
