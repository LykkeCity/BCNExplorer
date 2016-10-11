using System;
using System.Threading.Tasks;
using AzureRepositories.Asset;
using AzureStorage.Queue;
using Common;
using Common.Log;
using Core.Asset;
using Providers.BlockChainReader;
using Providers.TransportTypes.Asset;

namespace AssetScanner.QueueHandlers
{
    public class UpdateAssetDataCommandQueueConsumer:IStarter
    {
        private readonly IAssetDataQueueReader _queueReader;
        private readonly ILog _log;
        private readonly AssetReader _assetReader;
        private readonly IAssetDefinitionRepository _assetDefinitionRepository;

        public UpdateAssetDataCommandQueueConsumer(ILog log, AssetReader assetReader, IAssetDefinitionRepository assetDefinitionRepository, IAssetDataQueueReader queueReader)
        {
            _log = log;
            _assetReader = assetReader;
            _assetDefinitionRepository = assetDefinitionRepository;
            _queueReader = queueReader;

            _queueReader.RegisterPreHandler(async data =>
            {
                if (data == null)
                {
                    await _log.WriteInfo("UpdateAssetDataCommandQueueHandler", "InitQueues", null, "Queue had unknown message");
                    return false;
                }
                return true;
            });

            _queueReader.RegisterHandler<QueueRequestModel<UpdateAssetDataContext>>(
                UpdateAssetDataContext.Id, itm => UpdateAssetData(itm.Data));
        }

        public async Task UpdateAssetData(UpdateAssetDataContext context)
        {
            await _log.WriteInfo("UpdateAssetDataCommandQueueConsumer", "UpdateAssetData", context.ToJson(), $"Update {context.AssetDefinitionUrl} started {DateTime.Now} ");

            var assetData = await _assetReader.ReadAssetDataAsync(context.AssetDefinitionUrl);
            await _assetDefinitionRepository.InsertOrReplaceAsync(AssetDefinition.Create(assetData));
        }

        public void Start()
        {
            _queueReader.Start();
            _log.WriteInfo("UpdateAssetDataCommandQueueHandler", "Start", null,
                $"Started:{_queueReader.GetComponentName()}");
        }
    }
}
