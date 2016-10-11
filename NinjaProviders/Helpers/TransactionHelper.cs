using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.OpenAsset;

namespace Providers.Helpers
{
    public static class TransactionHelper
    {
        public static string GetTransactionId(this Transaction transaction)
        {
            return transaction.GetHash().AsBitcoinSerializable().Value.ToString();
        }

        public static Uri TryGetAssetDefinitionUrl(this Transaction transaction)
        {
            try
            {
                return transaction.GetColoredMarker().GetMetadataUrl();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Uri TryGetAssetDefinitionUrl(string hex)
        {
            return Transaction.Parse(hex).TryGetAssetDefinitionUrl();
        }
    }
}
