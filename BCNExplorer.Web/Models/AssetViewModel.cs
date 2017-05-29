using System;
using System.Collections.Generic;
using System.Linq;
using Core.Asset;

namespace BCNExplorer.Web.Models
{
    public class AssetViewModel
    {



        private const string DefaultAssetIconUrl = "/img/be/asset_default.jpg";
        public IEnumerable<string> AssetIds { get; set; }

        public bool IsColored { get; set; }

        public string ContactUrl { get; set; }

        public string NameShort { get; set; }

        public string Name { get; set; }

        public string Issuer { get; set; }

        public string Description { get; set; }

        public string DescriptionMime { get; set; }

        public string Type { get; set; }

        public int Divisibility { get; set; }

        public bool LinkToWebsite { get; set; }

        private string _iconUrl;
        public string IconUrl => !string.IsNullOrEmpty(_iconUrl) ? _iconUrl : DefaultAssetIconUrl;

        public string ImageUrl { get; set; }

        public string Version { get; set; }
        public string DefinitionUrl { get; set; }
        public bool IsVerified { get; set; }
        
        public bool ShowAssetDetailsLink { get; set; }

        public bool ShowAssetImage => !string.IsNullOrEmpty(ImageUrl);
        public static AssetViewModel Create(IAssetDefinition source)
        {
            return new AssetViewModel
            {
                AssetIds = source.AssetIds ?? Enumerable.Empty<string>(),
                ContactUrl = source.ContactUrl,
                Description = source.Description,
                DescriptionMime = source.DescriptionMime,
                Divisibility = source.Divisibility,
                _iconUrl = source.IconUrl,
                ImageUrl = source.ImageUrl,
                Issuer = source.Issuer,
                LinkToWebsite = source.LinkToWebsite,
                Name = source.Name,
                NameShort = source.NameShort,
                Type = source.Type,
                Version = source.Version,
                DefinitionUrl = source.AssetDefinitionUrl,
                IsVerified = source.IsVerified(),
                ShowAssetDetailsLink = true,
                IsColored = true
            };
        }

        private static AssetViewModel CreateBtcAsset()
        {
            return new AssetViewModel
            {
                AssetIds = Enumerable.Empty<string>(),
                Name = "Bitcoin",

                NameShort = "BTC",
                _iconUrl = "/img/assets/bitcoin.png",
                ShowAssetDetailsLink = false,
                IsColored = false
            };
        }

        public static Lazy<AssetViewModel> BtcAsset = new Lazy<AssetViewModel>(CreateBtcAsset);

        public static AssetViewModel CreateNotFoundAsset(string assetId)
        {
            return new AssetViewModel
            {
                AssetIds = new []{assetId},
                Name = assetId,
                ShowAssetDetailsLink = false,
                IsColored = true
            };
        }

        private sealed class AssetIdsEqualityComparer : IEqualityComparer<AssetViewModel>
        {
            public bool Equals(AssetViewModel x, AssetViewModel y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                if (x.AssetIds == null || y.AssetIds == null) return false;

                return x.AssetIds.Any(p => y.AssetIds.Contains(p));
            }

            public int GetHashCode(AssetViewModel obj)
            {
                return string.Join("_", (obj.AssetIds ?? Enumerable.Empty<string>())).GetHashCode();
            }
        }

        public static IEqualityComparer<AssetViewModel> AssetIdsComparer { get; } = new AssetIdsEqualityComparer();
    }
}