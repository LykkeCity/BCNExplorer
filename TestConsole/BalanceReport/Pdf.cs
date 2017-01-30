using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.IocContainer;
using Core.AddressService;
using Core.Asset;
using Core.BalanceReport;
using Core.Block;

namespace TestConsole.BalanceReport
{
    public static class Pdf
    {
        public static async Task Run(IoC container)
        {
            Console.WriteLine("Fetching data started ");
            var reportRender = container.GetObject<IReportRenderer>();
            var addressService = container.GetObject<IAddressService>();
            var blockService = container.GetObject<IBlockService>();
            var assetService = container.GetObject<IAssetService>();



            var atBlock = 446032;
            var blockHeader = await blockService.GetBlockHeaderAsync(atBlock.ToString());


            Console.WriteLine("Pdf rendering started ");

            var filePath = "./BalanceReport.pdf";

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var assetsToTrack = new[]
{
                "AWm6LaxuJgUQqJ372qeiUxXhxRWTXfpzog",
                "AXkedGbAH1XGDpAypVzA5eyjegX4FaCnvM",
                "AYeENupK7A9LZ5BsQiXnp22tHHquoASsFc",
                "AJPMQpygd8V9UCAxwFYYHYXLHJ7dUkQJ5w",
                "ASzmrSxhHjioWMYivoawap9yY4cxAfAMxR",
                "AKi5F8zPm7Vn1FhLqQhvLdoWNvWqtwEaig",
                "Ab8mNRBmrPJCmghHDoMsq26GP5vxm7hZpP"

            };


            var addr = new[] { "anMUe3LgGapNHxKsGxmtbsPpNeC33sa7y9a" , "anJBX5sKFK4vnbywKWE2NQa9xrvLJEqRAB2" };

            var clientBalance = ClientBalance.Create();

            foreach (var addressId in addr)
            {
                var balances = new List<AssetBalance>();

                var bal = await addressService.GetBalanceAsync(addressId, atBlock);

                balances.Add(new AssetBalance
                {
                    AssetId = "BTC",
                    Quantity = Convert.ToDecimal(BitcoinUtils.SatoshiToBtc(bal.BtcBalance))
                });

                foreach (var assetBalance in bal.ColoredBalances.Where(p => assetsToTrack.Contains(p.AssetId)))
                {
                    balances.Add(new AssetBalance
                    {
                        AssetId = assetBalance.AssetId,
                        Quantity = Convert.ToDecimal(assetBalance.Quantity)
                    });
                }

                clientBalance.Add(addressId, balances);

            }

            var assetDic = await assetService.GetAssetDefinitionDictionaryAsync();

            var fiatPrices = FiatRate.Create("USD", new Dictionary<string, decimal>
            {
                {"AJPMQpygd8V9UCAxwFYYHYXLHJ7dUkQJ5w", 0.981345m },//chf
                {"ASzmrSxhHjioWMYivoawap9yY4cxAfAMxR", 1.05204m },//eur
                {"AKi5F8zPm7Vn1FhLqQhvLdoWNvWqtwEaig", 1.23412m },//gbp
                {"Ab8mNRBmrPJCmghHDoMsq26GP5vxm7hZpP", 0.008546m}, //jpy
                {"AWm6LaxuJgUQqJ372qeiUxXhxRWTXfpzog", 1 },//usd
                {"BTC", 945.492m },//btc
                {"AYeENupK7A9LZ5BsQiXnp22tHHquoASsFc", 0.07967449m }//solar
            });

            using (var fileStream = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                reportRender.RenderBalance(fileStream, 
                    Client.Create("lp-slr@lykke.com", "Andrey Volkov"), 
                    blockHeader,
                    fiatPrices,
                    clientBalance, assetDic);
            }

            Console.WriteLine("Pdf rendering done ");
        } 
    }
}
