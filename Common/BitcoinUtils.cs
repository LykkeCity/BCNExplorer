using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class BitcoinUtils
    {
        public static double SatoshiToBtc(double satoshi)
        {
            return satoshi * 0.00000001;
        }

        public static double CalculateColoredAssetQuantity(double quantity, int divisibility )
        {
            return quantity*Math.Pow(10, - divisibility);
        }

        const string BtcFormatString = "### ### ### ### ##0.#################";
        public static string ToStringBtcFormat(this double quantity)
        {
            return quantity.ToString(BtcFormatString).Trim();
        }

        public static string ToStringBtcFormat(this decimal quantity)
        {
            return quantity.ToString(BtcFormatString).Trim();
        }
    }
}
