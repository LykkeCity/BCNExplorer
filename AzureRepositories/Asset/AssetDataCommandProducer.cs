using System.Threading.Tasks;
using AzureStorage.Queue;
using Common.Log;

namespace AzureRepositories.Asset
{
    public interface IAssetDataQueueReader : IQueueReader
    {
        
    }

    public class AssetDataQueueReader : QueueReader, IAssetDataQueueReader
    {
        public AssetDataQueueReader(IQueueExt queueExt, string componentName, int periodMs, ILog log) : base(queueExt, componentName, periodMs, log)
        {
        }
    }

    public class UpdateAssetDataContext
    {
        public const string Id = "CreateAssetDataContext";
        public string AssetDefinitionUrl { get; set; }
    }

    public class AssetDataCommandProducer
    {
        private readonly IQueueExt _queueExt;

        public AssetDataCommandProducer(IQueueExt queueExt)
        {
            _queueExt = queueExt;

            _queueExt.RegisterTypes(QueueType.Create(UpdateAssetDataContext.Id, typeof(QueueRequestModel<UpdateAssetDataContext>)));
        }

        public async Task CreateUpdateAssetDataCommand(params string[] urls)
        {
            foreach (var url in urls)
            {
                await _queueExt.PutMessageAsync(new QueueRequestModel<UpdateAssetDataContext>
                {
                    Data = new UpdateAssetDataContext
                    {
                        AssetDefinitionUrl = url
                    }
                });
            }
        } 
    }
}
