using System.Threading.Tasks;

namespace Core.Asset
{
    public interface IAssetChangesParsedBlock
    {
        string BlockHash { get; }
    }

    public class AssetChangesParsedBlock: IAssetChangesParsedBlock
    {
        public string BlockHash { get; set; }

        public static AssetChangesParsedBlock Create(string hash)
        {
            return new AssetChangesParsedBlock
            {
                BlockHash = hash
            };
        }
    }

    public interface IAssetChangesParsedBlockRepository
    {
        Task<bool> IsBlockExistsAsync(IAssetChangesParsedBlock block);
        Task AddBlockAsync(IAssetChangesParsedBlock block);
    }
}
