using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.IocContainer;
using Core.AddressService;
using Core.BalanceReport;
using Core.Block;

namespace TestConsole.BalanceReport
{
    public static class Pdf
    {
        public static async Task Run(IoC container)
        {
            var reportRender = container.GetObject<IReportRender>();
            var addressService = container.GetObject<IAddressService>();
            var blockService = container.GetObject<IBlockService>();

            var atBlock = 449650;
            var blockHeader = await blockService.GetBlockHeaderAsync(atBlock.ToString());

            var balances = new List<AssetBalance>
            {
                AssetBalance.Create(123, "asssetId"),

                AssetBalance.Create(324, "asssetId2")
            };

            //reportRender.RenderBalanceAsync(Client.Create("clientId", "address"), FiatPrice.Create("currencyName", null),
            //    balances);
            Console.WriteLine("Pdf");
        } 
    }
}
