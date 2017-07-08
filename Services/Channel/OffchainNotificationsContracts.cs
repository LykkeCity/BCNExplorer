using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Channel
{
    public class ChannelViewModelContract
    {
        public string AssetId { get; set; }
        public bool IsColored { get; set; }
        public string OpenTransactionId { get; set; }
        public string CloseTransactionId { get; set; }
        public IEnumerable<OffchainTransactionViewModelContract> OffchainTransactions { get; set; }
    }

    public class OffchainTransactionViewModelContract
    {
        public string TransactionId { get; set; }
        public DateTime DateTime { get; set; }
        public string HubAddress { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string AssetId { get; set; }
        public bool IsColored { get; set; }
        public decimal Address1Quantity { get; set; }
        public decimal Address2Quantity { get; set; }
    }

    public class PageOptionsRequestContract
    {
        public int? Skip { get; set; }

        public int? Take { get; set; }
    }
}
