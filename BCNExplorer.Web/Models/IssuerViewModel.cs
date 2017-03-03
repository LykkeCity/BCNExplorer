using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Core.Asset;

namespace BCNExplorer.Web.Models
{
    public class IssuerViewModel
    {
        public string Issuer { get; set; }
        public string IssuerWebsite { get; set; }

        public bool IsVerified { get; set; }

        public AssetDirectoryViewModel AssetDirectory { get; set; }

        public static IssuerViewModel Create(string issuer, 
            IEnumerable<IAssetDefinition> assetDefinitions,
            IDictionary<string, IAssetCoinholdersIndex> assetCoinholdersIndices,
            IDictionary<string, IAssetScore> assetScoresDictionaries)
        {
            if (assetDefinitions.Any())
            {
                return new IssuerViewModel
                {
                    IsVerified = true, //Temp Solution
                    IssuerWebsite = assetDefinitions.Where(p => !string.IsNullOrEmpty(p.IssuerWebsite())).Select(p => p.IssuerWebsite()).FirstOrDefault(),
                    Issuer = issuer,
                    AssetDirectory = AssetDirectoryViewModel.Create(assetDefinitions, assetCoinholdersIndices, assetScoresDictionaries)
                };
            }

            return null;
        }
    }
}