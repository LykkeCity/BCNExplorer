using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Providers.Contracts.Lykke;

namespace Providers.TransportTypes.Lykke
{
    public class LykkeAsset
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

        public static LykkeAsset Create(LykkeAssetContract source)
        {
            return new LykkeAsset
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
                Version = source.Version
            };
        }
    }
}
