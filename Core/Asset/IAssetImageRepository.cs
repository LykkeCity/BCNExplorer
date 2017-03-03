using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Asset
{
    public interface IAssetImage
    {
        IEnumerable<string> AssetIds { get; }
        string IconUrl { get; }
        string ImageUrl { get; }
    }

    public class AssetImage:IAssetImage
    {
        public IEnumerable<string> AssetIds { get; set; }
        public string IconUrl { get; set; }
        public string ImageUrl { get; set; }

        public static AssetImage Create(IEnumerable<string> assetIds, IImageSaveResult icon, IImageSaveResult image)
        {
            return new AssetImage
            {
                AssetIds = assetIds,
                IconUrl = icon.CachedUrl,
                ImageUrl = image.CachedUrl
            };
        }
    }

    public interface IAssetImageRepository
    {
        Task<IEnumerable<IAssetImage>> GetAllAsync();
        Task InsertOrReplaceAsync(IAssetImage assetImage);
    }
}
