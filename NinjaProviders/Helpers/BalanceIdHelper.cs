using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.DataEncoders;
using NBitcoin.Indexer;

namespace Providers.Helpers
{
    public static class BalanceIdHelper
    {
        public static BalanceId Parse(string key, Network network)
        {
            if (BitcoinAddressHelper.IsBitcoinColoredAddress(key, network))
            {
                return new BalanceId(new  BitcoinColoredAddress(key, network));
            }

            if (BitcoinAddressHelper.IsBitcoinPubKeyAddress(key, network))
            {
                return new BalanceId(new BitcoinPubKeyAddress(key, network));
            }

            if (BitcoinAddressHelper.IsBitcoinScriptAddress(key, network))
            {
                return new BalanceId(new BitcoinScriptAddress(key, network));
            }

            if (key.Length > 3 && key.Length < 5000 && key.StartsWith("0x"))
            {
                return new BalanceId(new Script(Encoders.Hex.DecodeData(key.Substring(2))));
            }
            if (key.Length > 3 && key.Length < 5000 && key.StartsWith("W-"))
            {
                return new BalanceId(key.Substring(2));
            }
            var data = Network.CreateFromBase58Data(key, network);
            if (!(data is IDestination))
            {
                throw new FormatException("Invalid base58 type");
            }

            return new BalanceId((IDestination)data);
        }
    }
}
