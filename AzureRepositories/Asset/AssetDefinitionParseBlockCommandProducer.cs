using System.Threading.Tasks;
using AzureStorage.Queue;

namespace AzureRepositories.AssetDefinition
{
    public class AssetDefinitionParseBlockContext
    {
        public const string Id = "AssetParseBlockDataContext";

        public string BlockHash { get; set; }
    }

    public class AssetDefinitionParseBlockCommandProducer
    {
        private readonly IQueueExt _queueExt;

        public AssetDefinitionParseBlockCommandProducer(IQueueExt queueExt)
        {
            _queueExt = queueExt;

            _queueExt.RegisterTypes(QueueType.Create(AssetDefinitionParseBlockContext.Id, typeof(QueueRequestModel<AssetDefinitionParseBlockContext>)));
        }

        public async Task CreateParseBlockCommand(params string[] blockHashs)
        {
            foreach (var blockHash in blockHashs)
            {
                await _queueExt.PutMessageAsync(new QueueRequestModel<AssetDefinitionParseBlockContext>
                {
                    Data = new AssetDefinitionParseBlockContext
                    {
                        BlockHash = blockHash
                    }
                });
            }
        }
    }
}
