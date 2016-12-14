using System;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    public static class HerfindahlIndex
    {
        public static double Calculate(IEnumerable<double> shares)
        {
            return shares?.Sum(p => Math.Pow(p, 2)) ?? 0;
        }

        public static double CalculateShare(double part, double total)
        {
            if (total != 0)
            {
                return part/total;
            }

            return 1;

        }
    }
}
