using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCNExplorer.Web.Models;
using Common;
using Common.IocContainer;
using Core.AssetBlockChanges;
using Core.AssetBlockChanges.Mongo;
using Providers.Providers.Asset;
using Providers.Providers.Ninja;
using SQLRepositories.Context;

namespace TestConsole
{
    public static class CheckBalance
    {
        public static async Task Run(IoC container)
        {
            var db = container.GetObject<BcnExplolerFactory>();
            var _assetProvider = container.GetObject<AssetProvider>();
            var _addressProvider = container.GetObject<AddressProvider>();
            var _balanceChangesRepository = container.GetObject<IAssetBalanceChangesRepository>();
            var id = "AXkedGbAH1XGDpAypVzA5eyjegX4FaCnvM";
            var asset = await _assetProvider.GetAssetAsync(id);

            var result = AssetCoinholdersViewModel.Create(
                AssetViewModel.Create(asset),
                await _balanceChangesRepository.GetSummaryAsync(asset.AssetIds.ToArray()));
            var file = "./diff.txt";
            File.Delete(file);
            var counter = result.AddressSummaries.Count();
            foreach (var sum in result.AddressSummaries.OrderByDescending(p=>p.Balance))
            {
                try
                {
                    Console.WriteLine(counter);
                    counter--;
                    var ninjablance = await _addressProvider.GetAddressAsync(sum.Address);
                    var assetQuntity = ninjablance.Assets.Single(p => p.AssetId == id).Quantity;
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
        }
    }
}
