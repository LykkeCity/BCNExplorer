using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Asset
{
    public interface IAssetDefinition
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

        bool IsVerified { get; }
    }

    
    public sealed class AssetDefinitionUrlEqualityComparer : IEqualityComparer<IAssetDefinition>
    {
        public bool Equals(IAssetDefinition x, IAssetDefinition y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return string.Equals(x.AssetDefinitionUrl, y.AssetDefinitionUrl);
        }

        public int GetHashCode(IAssetDefinition obj)
        {
            return (obj.AssetDefinitionUrl != null ? obj.AssetDefinitionUrl.GetHashCode() : 0);
        }
    }

    public interface IAssetDefinitionRepository
    {
        Task<IEnumerable<IAssetDefinition>> GetAllAsync();
        Task<IEnumerable<IAssetDefinition>> GetAllEmptyAsync();
        Task InsertOrReplaceAsync(params IAssetDefinition[] assetsDefinition);
        Task InsertEmptyAsync(string defUrl);
        Task<bool> IsAssetExistsAsync(string defUrl);
        Task RemoveEmptyAsync(params string[] defUrls);
        Task UpdateAssetAsync(IAssetDefinition assetDefinition);
    }
}
