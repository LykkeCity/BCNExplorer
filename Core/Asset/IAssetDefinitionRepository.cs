using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Asset
{
    public interface IAsset
    {
        IEnumerable<string> AssetIds { get; }

        string ContactUrl { get; }

        string NameShort { get; }

        string Name { get; }

        string Issuer { get; }

        string Description { get; }

        string DescriptionMime { get; }

        string Type { get; }

        int Divisibility { get; }

        bool LinkToWebsite { get; }

        string IconUrl { get; }

        string ImageUrl { get; }

        string Version { get; }
         string AssetDefinitionUrl { get; }
    }

    public interface IAssetDefinitionRepository
    {
        Task<IEnumerable<IAsset>> GetAllAsync();
        Task InsertOrReplaceAsync(params IAsset[] assets);
    }
}
