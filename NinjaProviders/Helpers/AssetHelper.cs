using System;
using NBitcoin;
using NBitcoin.DataEncoders;
using NBitcoin.OpenAsset;

namespace Providers.Helpers
{
    public static class AssetHelper
    {
        public static Uri TryGetAssetDefinitionUrl(string hex)
        {
            try
            {
                //hex = "01000000025246ac7685640b20f2c07ebaa37e743ccbf43866548dcf0a61eb19fdb051d803020000006a4730440220522f38e6688e7c939dc16bcb6ed63a8b40ac41137b9944f9efb5a693c8fc5e2602200b3994a8032d3f1aad81e006d6b695dcb7f53cf81e998d00f11d5eff59f5c42901210234c3cb8e5266357163843145ecfbd790430acb4f4c8f7bb00f5e8cf5231133f6ffffffff5246ac7685640b20f2c07ebaa37e743ccbf43866548dcf0a61eb19fdb051d803030000006a47304402205a7654789358e4531ae4dc0a64e360ca3700edbd5045bedcc60f47d8e0a2b445022012d219fe04c3a5b9cf63000b7df0bbe3dea4c3006f2fed677ea8d8f4f591023401210234c3cb8e5266357163843145ecfbd790430acb4f4c8f7bb00f5e8cf5231133f6ffffffff040000000000000000146a124f410100028094ebdc03c0cfe4ceb5a00600580200000000000017a914a0cf8310e1052a92e19cbda9665331b69e470b888758020000000000001976a914034a5b5d08bba66f6e660c9e63d8c2675360c23888ac3d154621000000001976a914034a5b5d08bba66f6e660c9e63d8c2675360c23888ac00000000";
                var tx = Transaction.Parse(hex);
                
                var color = tx.GetColoredMarker();

                return GetMetadataUrl(color);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static Uri GetMetadataUrl(ColorMarker cm)
        {
            if (cm.Metadata == null || cm.Metadata.Length == 0)
                return (Uri)null;
            string str = Encoders.ASCII.EncodeData(cm.Metadata);
            if (!str.StartsWith("u="))
                return (Uri)null;
            Uri result = (Uri)null;
            Uri.TryCreate(str.Substring(2), UriKind.Absolute, out result);
            return result;
        }
    }


}
