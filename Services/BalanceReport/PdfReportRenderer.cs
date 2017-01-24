using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Core.BalanceReport;
using Core.Block;

namespace Services.BalanceReport
{
    public class PdfReportRenderer:IReportRender
    {
        public async Task<Stream> RenderBalanceAsync(IClient client, IBlockHeader reportedAtBlock, IFiatPrices fiatPrices, IEnumerable<IAssetBalance> balances)
        {
            throw new NotImplementedException();
        }
    }
}
