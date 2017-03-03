using System;
using System.Linq;
using System.Threading.Tasks;
using AzureRepositories.Asset;
using AzureStorage.Queue;
using Common;
using Common.Log;
using Core.Asset;

namespace AssetDefinitionScanner.QueueHandlers
{
    public class AssetImagesCommandQueueConsumer:IStarter
    {
        private readonly IAssetImageQueueReader _queueReader;
        private readonly ILog _log;
        private readonly IAssetImageCacher _assetImageCacher;
        private readonly IAssetImageRepository _assetImageRepository;

        public AssetImagesCommandQueueConsumer(ILog log, IAssetImageQueueReader queueReader, 
            IAssetImageCacher assetImageCacher, 
            IAssetImageRepository assetImageRepository)
        {
            _log = log;
            _queueReader = queueReader;
            _assetImageCacher = assetImageCacher;
            _assetImageRepository = assetImageRepository;

            _queueReader.RegisterPreHandler(async data =>
            {
                if (data == null)
                {
                    await _log.WriteInfo("UpdateAssetImagesCommandQueueConsumer", "InitQueues", null, "Queue had unknown message");
                    return false;
                }
                return true;
            });

            _queueReader.RegisterHandler<QueueRequestModel<AssetImageContext>>(
                AssetImageContext.Id, itm => UpdateAssetImage(itm.Data));
        }

        public async Task UpdateAssetImage(AssetImageContext context)
        {
            try
            {
                await _log.WriteInfo("UpdateAssetImagesCommandQueueConsumer", "UpdateAssetImage", context.ToJson(), "Started");

                var iconResult = await _assetImageCacher.SaveAssetIconAsync(context.IconUrl, context.AssetIds.First());
                var imageResult = await _assetImageCacher.SaveAssetImageAsync(context.ImageUrl, context.AssetIds.First());

                await _assetImageRepository.InsertOrReplaceAsync(
                    AssetImage.Create(context.AssetIds, 
                    iconResult,
                    imageResult));

                await _log.WriteInfo("UpdateAssetImagesCommandQueueConsumer", "UpdateAssetImage", context.ToJson(), "Done");
            }
            catch (Exception e)
            {
                await _log.WriteError("UpdateAssetImagesCommandQueueConsumer", "UpdateAssetImage", context.ToJson(), e);
            }
        }

        public void Start()
        {
            _queueReader.Start();
            _log.WriteInfo("UpdateAssetImagesCommandQueueConsumer", "Start", null,
                $"Started:{_queueReader.GetComponentName()}");
        }
    }
}
