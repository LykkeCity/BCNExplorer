using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Asset
{
    public interface IAssetScore
    {
        IEnumerable<string> AssetIds { get; }
        double Score { get; }
    }

    public class AssetScore:IAssetScore
    {
        public IEnumerable<string> AssetIds { get; set; }
        public double Score { get; set; }

        public static AssetScore Create(IEnumerable<string> assetIds, double score)
        {
            return new AssetScore
            {
                AssetIds = assetIds,
                Score = score
            };
        }
    }

    public interface IAssetScoreRepository
    {
        Task InsertOrReplaceAsync(IAssetScore assetScore);
        Task<IEnumerable<IAssetScore>> GetAllAsync();
    }
}
