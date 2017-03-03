using System;
using System.Threading.Tasks;
using AzureRepositories.Asset;
using AzureRepositories.AssetDefinition;
using AzureStorage.Queue;
using Common;
using Common.Log;
using Core.Asset;
using Providers.BlockChainReader;
using Providers.TransportTypes.Asset;

namespace AssetDefinitionScanner.QueueHandlers
{
    public class AssetDataCommandQueueConsumer:IStarter
    {
        private readonly IAssetDataQueueReader _queueReader;
        private readonly ILog _log;
        private readonly AssetReader _assetReader;
        private readonly IAssetDefinitionRepository _assetDefinitionRepository;
        private readonly AssetImageCommandProducer _assetImageCommandProducer;

        public AssetDataCommandQueueConsumer(ILog log, AssetReader assetReader, 
            IAssetDefinitionRepository assetDefinitionRepository, 
            IAssetDataQueueReader queueReader, 
            AssetImageCommandProducer assetImageCommandProducer)
        {
            _log = log;
            _assetReader = assetReader;
            _assetDefinitionRepository = assetDefinitionRepository;
            _queueReader = queueReader;
            _assetImageCommandProducer = assetImageCommandProducer;

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
            try
            {
                await _log.WriteInfo("UpdateAssetDataCommandQueueConsumer", "UpdateAssetData", context.ToJson(), "Started");
                var assetData = await _assetReader.ReadAssetDataAsync(context.AssetDefinitionUrl);
                if (assetData != null)
                {
                    await _assetDefinitionRepository.InsertOrReplaceAsync(AssetDefinition.Create(assetData));
                    await _assetImageCommandProducer.CreateUpdateAssetImageCommand(assetData.AssetIds, assetData.IconUrl,
                            assetData.ImageUrl);
                }

                await _log.WriteInfo("UpdateAssetDataCommandQueueConsumer", "UpdateAssetData", context.ToJson(), "Done");
            }
            catch (Exception e)
            {
                await _log.WriteError("UpdateAssetDataCommandQueueConsumer", "UpdateAssetData", context.ToJson(), e);

                if (!await _assetDefinitionRepository.IsAssetExistsAsync(context.AssetDefinitionUrl))
                {
                    await _assetDefinitionRepository.InsertEmptyAsync(context.AssetDefinitionUrl);
                }
            }

        }

        public void Start()
        {
            _queueReader.Start();
            _log.WriteInfo("UpdateAssetDataCommandQueueHandler", "Start", null,
                $"Started:{_queueReader.GetComponentName()}");
        }
    }
}
