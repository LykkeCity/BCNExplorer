using System.Linq;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Common.Log;
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
        private readonly ILog _log;

        public AssetCoinholdersIndexesCommandProducer(IQueueExt queueExt, ILog log)
        {
            _queueExt = queueExt;
            _log = log;

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

                await
                    _log.WriteInfo("AssetCoinholdersIndexesCommandProducer", "CreateAssetCoinholdersUpdateIndexCommand",
                        asset.AssetIds.FirstOrDefault(), "Done");
            }
        }

        //public async Task CreateAssetCoinholdersUpdateIndexCommand(params string[] assetIds)
        //{
        //    foreach (var assetID in assetIds)
        //    {
        //        await _queueExt.PutMessageAsync(new QueueRequestModel<AssetCoinholdersUpdateIndexCommand>
        //        {
        //            Data = new AssetCoinholdersUpdateIndexCommand
        //            {
        //                AssetId = assetID
        //            }
        //        });
        //    }
        //}
    }
}
