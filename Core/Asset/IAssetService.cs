using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Asset
{
    public interface IAssetService
    {
        Task<IAsset> GetAssetAsync(string assetId);
        Task<IDictionary<string, IAsset>> GetAssetDictionaryAsync();
        Task<IEnumerable<IAsset>> GetAssetsAsync();
    }
}
