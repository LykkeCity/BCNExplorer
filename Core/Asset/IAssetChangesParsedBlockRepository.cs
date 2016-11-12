using System.Threading.Tasks;

namespace Core.Asset
{

    public interface IAssetChangesParsedBlockRepository
    {
        Task<int> GetLastParsetBlockHeightAsync();
    }
}
