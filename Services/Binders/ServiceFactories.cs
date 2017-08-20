using AzureStorage.Blob;
using Common.Log;
using Core.Settings;
using Services.Asset;

namespace Services.Binders
{
    public static class ServiceFactories
    {
        public static AssetImageCacher CreateIAssetImageCacher(BaseSettings baseSettings, ILog log)
        {
            return new AssetImageCacher(new AzureBlobStorage(baseSettings.Db.AssetsConnString), log);
        }
    }
}
