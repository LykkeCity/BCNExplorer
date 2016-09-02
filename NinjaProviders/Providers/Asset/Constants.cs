using System.Collections.Generic;

namespace Providers.Providers.Asset
{
    public class Constants
    {
        public static IEnumerable<string> AssetDefinitonUrls => new[]
        {
            "https://www.lykkex.com/lkeUSD",
            "https://www.lykkex.com/lkeEUR",
            "https://www.lykkex.com/lkeCHF",
            "https://lykke.com/asset/LKK",
            "https://www.lykkex.com/LykkeCorpEquity"
        };
    }
}
