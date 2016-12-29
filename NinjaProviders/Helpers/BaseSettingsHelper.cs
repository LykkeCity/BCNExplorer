using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Settings;
using NBitcoin;

namespace Providers.Helpers
{
    public static class BaseSettingsHelper
    {
        public static Network UsedNetwork(this BaseSettings baseSettings)
        {
            try
            {
                return Network.GetNetwork(baseSettings.Network);
            }
            catch (Exception)
            {
                return Network.Main;
            }
        }
    }
}
