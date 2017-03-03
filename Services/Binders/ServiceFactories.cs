using AzureStorage.Blob;
using Common.Log;
using Core.Settings;
using Lykke.EmailSenderProducer;
using Services.Asset;

namespace Services.Binders
{
    public static class ServiceFactories
    {
        public static EmailSenderProducer CreateEmailSenderProducer(BaseSettings baseSettings, ILog log)
        {
            return new EmailSenderProducer(baseSettings.ServiceBusEmailSettings, log);
        }

        public static AssetImageCacher CreateIAssetImageCacher(BaseSettings baseSettings, ILog log)
        {
            return new AssetImageCacher(new AzureBlobStorage(baseSettings.Db.AssetsConnString), log);
        }
    }
}
