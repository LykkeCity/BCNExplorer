using System.Linq;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Core.Asset;

namespace AzureRepositories.AssetCoinHolders
{
    public class AssetCoinholdersUpdateIndexCommand
    {
        public const string Id = "AssetCoinholdersUpdateIndexCommand";

        public string AssetId { get; set; }
    }


    public class AssetCoinholdersIndexesCommandProducer
    {
        private readonly IQueueExt _queueExt;

        public AssetCoinholdersIndexesCommandProducer(IQueueExt queueExt)
        {
            _queueExt = queueExt;

            _queueExt.RegisterTypes(QueueType.Create(AssetCoinholdersUpdateIndexCommand.Id, typeof(QueueRequestModel<AssetCoinholdersUpdateIndexCommand>)));
        }


        public async Task CreateAssetCoinholdersUpdateIndexCommand(params IAssetDefinition[] assetsDefinition)
        {
            foreach (var asset in assetsDefinition)
            {
                await _queueExt.PutMessageAsync(new QueueRequestModel<AssetCoinholdersUpdateIndexCommand>
                {
                    Data = new AssetCoinholdersUpdateIndexCommand
                    {
                        AssetId = asset.AssetIds.FirstOrDefault()
                    }
                });
            }
        }
    }
}
