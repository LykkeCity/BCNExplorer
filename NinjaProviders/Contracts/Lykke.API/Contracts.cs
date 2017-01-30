using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Providers.Contracts.Lykke.API
{
    public class AssetContract
    {
        public string id { get; set; }

        public string name { get; set; }

        public string bitcoinAssetId { get; set; }

        public string bitcoinAssetAddress { get; set; }

        public string symbol { get; set; }

        public int accuracy { get; set; }
    }

    public class AssetPairContract
    {
        public string id { get; set; }

        public string name { get; set; }

        public int accuracy { get; set; }

        public string baseAssetId { get; set; }

        public string quotingAssetId { get; set; }
    }

    public class AssetPairRateContract
    {
        public string id { get; set; }
        public decimal bid { get; set; }
        public decimal ask { get; set; }
    }
}
