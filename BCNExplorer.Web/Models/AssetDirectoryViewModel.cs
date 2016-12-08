using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Common;
using Core.Asset;

namespace BCNExplorer.Web.Models
{
    public class AssetDirectoryViewModel
    {
        public IEnumerable<Asset> Assets { get; set; }

        public static AssetDirectoryViewModel Create(IEnumerable<IAssetDefinition> assetDefinitions,
            IDictionary<string, IAssetCoinholdersIndex> assetCoinholdersIndices)
        {
            return new AssetDirectoryViewModel
            {
                Assets = assetDefinitions.Select(p =>
                {
                    IAssetCoinholdersIndex index = null;

                    var assetId = p.AssetIds.FirstOrDefault();
                    if (assetCoinholdersIndices.ContainsKey(assetId))
                    {
                        index = assetCoinholdersIndices[assetId];
                    }
                    return Asset.Create(p, index);
                })
                    .OrderByDescending(p => p.Spread)
                    .ThenByDescending(p => p.IsVerified)
                    .ThenBy(p => p.Name).ToList()
            };
        }

    public class Asset
        {
            public IEnumerable<string> AssetIds { get; set; }

            public string ContactUrl { get; set; }

            public string NameShort { get; set; }

            public string Name { get; set; }

            public string Issuer { get; set; }

            public string Description { get; set; }

            public string DescriptionMime { get; set; }

            public string Type { get; set; }

            public int Divisibility { get; set; }

            public bool LinkToWebsite { get; set; }

            public string IconUrl { get; set; }

            public string ImageUrl { get; set; }

            public string Version { get; set; }

            public bool IsVerified { get; set; }
            public int? CoinholdersCount { get; set; }
            private double? TotalQuantity { get; set; }
            public double? TotalColored => TotalQuantity != null? (double?)BitcoinUtils.CalculateColoredAssetQuantity(TotalQuantity.Value, Divisibility):null;
            
            public double Spread { get; set; }

            public static Asset Create(IAssetDefinition source, IAssetCoinholdersIndex index)
            {
                return new Asset
                {
                    AssetIds = source.AssetIds ?? Enumerable.Empty<string>(),
                    ContactUrl = source.ContactUrl,
                    Description = source.Description,
                    DescriptionMime = source.DescriptionMime,
                    Divisibility = source.Divisibility,
                    IconUrl = source.IconUrl,
                    ImageUrl = source.ImageUrl,
                    Issuer = source.Issuer,
                    LinkToWebsite = source.LinkToWebsite,
                    Name = source.Name,
                    NameShort = source.NameShort,
                    Type = source.Type,
                    Version = source.Version,
                    IsVerified = source.IsVerified,
                    CoinholdersCount = index?.CoinholdersCount,
                    TotalQuantity = index?.TotalQuantity,
                    Spread = index?.Spread ?? 0
                };
            }
            

        }
    }
}