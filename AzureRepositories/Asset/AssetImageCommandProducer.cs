using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Common.Log;

namespace AzureRepositories.Asset
{
    public interface IAssetImageQueueReader : IQueueReader
    {

    }

    public class AssetImageQueueReader : QueueReader, IAssetImageQueueReader
    {
        public AssetImageQueueReader(IQueueExt queueExt, string componentName, int periodMs, ILog log) : base(queueExt, componentName, periodMs, log)
        {
        }
    }

    public class AssetImageContext
    {
        public const string Id = "AssetImageContext";
        public IEnumerable<string> AssetIds { get; set; }

        public string IconUrl { get; set; }

        public string ImageUrl { get; set; }
    }

    public class AssetImageCommandProducer
    {
        private readonly IQueueExt _queueExt;

        public AssetImageCommandProducer(IQueueExt queueExt)
        {
            _queueExt = queueExt;

            _queueExt.RegisterTypes(QueueType.Create(AssetImageContext.Id, typeof(QueueRequestModel<AssetImageContext>)));
        }

        public async Task CreateUpdateAssetImageCommand(IEnumerable<string> assetIds, string iconUrl, string imageUrl)
        {
            await _queueExt.PutMessageAsync(new QueueRequestModel<AssetImageContext>
            {
                Data = new AssetImageContext
                {
                    AssetIds = assetIds,
                    IconUrl = iconUrl,
                    ImageUrl = imageUrl
                }
            });
        }
    }
}
