using System;
using System.Collections.Generic;
using System.IO;
using Core.BalanceReport;
using Core.Block;

namespace Services.BalanceReport
{
    public class PdfReportRenderer:IReportRender
    {
        public Stream RenderBalance(IClient client, IBlockHeader reportedAtBlock, IFiatPrices fiatPrices, IEnumerable<IAssetBalance> balances)
        {
            throw new NotImplementedException();
        }
    }
}
