using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly IQueueReader _queueReader;
        private readonly ILog _log;
        private readonly AssetReader _assetReader;
        private readonly IAssetRepository _assetRepository;

        public UpdateAssetDataCommandQueueConsumer(ILog log, AssetReader assetReader, IAssetRepository assetRepository, IQueueReader queueReader)
        {
            _log = log;
            _assetReader = assetReader;
            _assetRepository = assetRepository;
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
            await _log.WriteInfo("AssetUpdaterFunctions", "UpdateAssets", "CreateAssetData", $"Update {context.AssetDefinitionUrl} started {DateTime.Now} ");

            var assetData = await _assetReader.ReadAssetDataAsync(context.AssetDefinitionUrl);
            await _assetRepository.InsertOrReplaceAsync(AssetDefinition.Create(assetData));
        }

        public void Start()
        {
            _queueReader.Start();
            _log.WriteInfo("UpdateAssetDataCommandQueueHandler", "Start", null,
                $"Started:{_queueReader.GetComponentName()}");
        }
    }
}
