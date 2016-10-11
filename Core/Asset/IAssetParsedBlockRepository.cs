using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Asset
{
    public interface IAssetParsedBlock
    {
        string BlockHash { get; }
    }

    public class AssetParsedBlock: IAssetParsedBlock
    {
        public string BlockHash { get; set; }

        public static AssetParsedBlock Create(string hash)
        {
            return new AssetParsedBlock
            {
                BlockHash = hash
            };
        }
    }

    public interface IAssetParsedBlockRepository
    {
        Task<bool> IsBlockExistsAsync(IAssetParsedBlock block);
        Task AddBlockAsync(IAssetParsedBlock block);
    }
}
