using System;

namespace BCNExplorer.Web.Helpers
{
    public static class BcnDateHelper
    {
        public static string ToStringBcnExplolerFormat(this DateTime date)
        {
            if (DateTime.Now - date > TimeSpan.FromDays(7))
            {
                return date.ToString("MMMM dd, yyyy h:mm tt");
            }

            return date.ToString("f");
        }
    }
}