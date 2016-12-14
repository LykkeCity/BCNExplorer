using System;
using System.Collections.Generic;
using Core.Asset;

namespace Providers.Helpers
{
    public static class AssetScoreHelper
    {
        public static double CalculateAssetScore(IAssetDefinition assetDefinition, IAssetCoinholdersIndex indexer,
            IEnumerable<IAssetCoinholdersIndex> allcoinholders)
        {
            var isVerified = assetDefinition?.IsVerified;
            throw new NotImplementedException();
        }

        private static double CalculateMinMaxRank(double value, double min, double max)
        {
            return (value - min)/(max - min);
        }
    }
}
