using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using Common.Log;
using Core.Asset;
using Flurl.Http;

namespace Services.Asset
{

    public class ImageSaveResult:IImageSaveResult
    {
        public bool Saved { get; set; }
        public string CachedUrl { get; set; }

        public static ImageSaveResult Ok(string url)
        {
            return new ImageSaveResult
            {
                Saved = true,
                CachedUrl = url
            };
        }

        public static ImageSaveResult Fail()
        {
            return new ImageSaveResult
            {
               Saved = false
            };
        }
    }

    public class AssetImageCacher:IAssetImageCacher
    {
        private readonly IBlobStorage _blobStorage;
        private readonly ILog _log;

        private const string IconContainer = "icons";
        private const string ImageContainer = "images";

        public AssetImageCacher(IBlobStorage blobStorage, ILog log)
        {
            _blobStorage = blobStorage;
            _log = log;
        }

        public async Task<IImageSaveResult> SaveAssetIconAsync(string url, string assetId)
        {
            return await Save(url, IconContainer, assetId);
        }

        public async Task<IImageSaveResult> SaveAssetImageAsync(string url, string assetId)
        {
            return await Save(url, ImageContainer, assetId);
        }

        private async Task<IImageSaveResult> Save(string url, string container, string assetId)
        {
            if (string.IsNullOrEmpty(url))
            {
                return ImageSaveResult.Fail();
            }
            try
            {
                var key = GenerateKeyName(assetId, GetImageExtension(url));

                var resp = await url.GetAsync();

                await _blobStorage.SaveBlobAsync(container, key, await resp.Content.ReadAsStreamAsync());

                return ImageSaveResult.Ok(_blobStorage.GetBlobUrl(container, key));
            }
            catch (Exception e)
            {
                await _log.WriteError("AssetImageCacher", "Save", url, e);

                return ImageSaveResult.Fail();
            }
        }

        private string GetImageExtension(string url)
        {
            var uri = new Uri(url);
            return Path.GetExtension(uri.AbsolutePath);
        }

        private string GenerateKeyName(string assetId, string extension)
        {
            return assetId + "." + extension;
        }
    }
}
