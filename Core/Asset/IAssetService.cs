using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Asset
{
    public interface IAssetService
    {
        Task<IAssetDefinition> GetAssetAsync(string assetId);
        Task<IAssetDefinition> GetAssetDefinitionByDefUrlAsync(string url);
        Task<IDictionary<string, IAssetDefinition>> GetAssetDefinitionDictionaryAsync();
        Task<IEnumerable<IAssetDefinition>> GetAssetDefinitionsAsync();
        Task<IDictionary<string, IAssetCoinholdersIndex>> GetAssetCoinholdersIndexAsync();
        Task<IDictionary<string, IAssetScore>> GetAssetScoreDictionaryAsync();
    }
}
