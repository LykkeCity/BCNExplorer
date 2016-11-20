using System.Threading.Tasks;
using AzureRepositories.AssetDefinition;
using AzureStorage.Queue;

namespace AzureRepositories.AssetCoinHolders
{
    public class AssetChangesParseBlockContext
    {
        public const string Id = "AssetCoinHoldersParseBlockContext";

        public int BlockHeight { get; set; }
        public int Attempt { get; set; }
    }

    public class AssetChangesParseBlockCommandProducer
    {
        private readonly IQueueExt _queueExt;

        public AssetChangesParseBlockCommandProducer(IQueueExt queueExt)
        {
            _queueExt = queueExt;
            _queueExt.RegisterTypes(QueueType.Create(AssetChangesParseBlockContext.Id, typeof(QueueRequestModel<AssetChangesParseBlockContext>)));
        }

        public async Task CreateParseBlockCommand(int blockHeight, int attempt = 0)
        {
            await _queueExt.PutMessageAsync(new QueueRequestModel<AssetChangesParseBlockContext>
            {
                Data = new AssetChangesParseBlockContext
                {
                    BlockHeight = blockHeight,
                    Attempt = attempt
                }
            });
        }
    }
}
