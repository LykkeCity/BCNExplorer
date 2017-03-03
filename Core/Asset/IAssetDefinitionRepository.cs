using System;
using System.Collections.Generic;
using System.Linq;
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

        string IconUrl { get; set; }

        string ImageUrl { get; set; }

        string Version { get; }
        string AssetDefinitionUrl { get; }
    }

    public static class AssetHelper
    {
        public static bool IsVerified(this IAssetDefinition assetDefinition)
        {
            var url = assetDefinition.AssetDefinitionUrl ?? "";
            Uri uriResult;
            var isHttps = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            var isCoinPrismDomain = url.Contains("cpr.sm");

            return isHttps && !isCoinPrismDomain;
        }

        public static string IssuerWebsite(this IAssetDefinition assetDefinition)
        {
            var url = assetDefinition.ContactUrl ?? "";

            Uri uriResult;
            var isCorrectUrl = Uri.TryCreate(url, UriKind.Absolute, out uriResult);

            if (isCorrectUrl)
            {
                return uriResult.Scheme + Uri.SchemeDelimiter + uriResult.Host;
            }

            return null;
        }

        public static bool IsValid(this IAssetDefinition assetDefinition)
        {
            return assetDefinition.AssetIds.Any() && assetDefinition.AssetIds.All(x => x != null);
        }
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
