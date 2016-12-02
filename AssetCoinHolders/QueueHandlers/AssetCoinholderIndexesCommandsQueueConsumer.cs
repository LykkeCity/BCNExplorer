using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureRepositories.AssetCoinHolders;
using AzureRepositories.QueueReaders;
using AzureStorage.Queue;
using Common;
using Common.Log;
using Core.Asset;
using Core.AssetBlockChanges.Mongo;

namespace AssetCoinHoldersScanner.QueueHandlers
{
    public class AssetCoinholderIndexesCommandsQueueConsumer : IStarter
    {
        private readonly ILog _log;
        private readonly ICoinholderIndexesQueueReader _queueReader;
        private readonly IAssetCoinholdersIndexRepository _assetCoinholdersIndexRepository;
        private readonly IAssetBalanceChangesRepository _balanceChangesRepository;
        private readonly IAssetService _assetService;

        public AssetCoinholderIndexesCommandsQueueConsumer(ILog log, 
            ICoinholderIndexesQueueReader queueReader,
            IAssetCoinholdersIndexRepository assetCoinholdersIndexRepository, 
            IAssetBalanceChangesRepository balanceChangesRepository, 
            IAssetService assetService)
        {
            _log = log;
            _queueReader = queueReader;
            _assetCoinholdersIndexRepository = assetCoinholdersIndexRepository;
            _balanceChangesRepository = balanceChangesRepository;
            _assetService = assetService;

            _queueReader.RegisterPreHandler(async data =>
            {
                if (data == null)
                {
                    await _log.WriteInfo("CoinholderIndexesCommandsQueueConsumer", "InitQueues", null, "Queue had unknown message");
                    return false;
                }
                return true;
            });

            
            _queueReader.RegisterHandler<QueueRequestModel<AssetCoinholdersUpdateIndexCommand>>(
                AssetCoinholdersUpdateIndexCommand.Id, itm => UpdateCoinholersIndex(itm.Data));
        }

        private async Task UpdateCoinholersIndex(AssetCoinholdersUpdateIndexCommand context)
        {
            await _log.WriteInfo("AssetCoinholderIndexesCommandsQueueConsumer", "UpdateCoinholersIndex", context.ToJson(), "Started");

            var asset = await _assetService.GetAssetAsync(context.AssetId);
            if (asset != null)
            {
                var balanceSummary = await _balanceChangesRepository.GetSummaryAsync(asset.AssetIds.ToArray());

                await _assetCoinholdersIndexRepository.InserOrReplaceAsync(AssetCoinholdersIndex.Create(balanceSummary));
            }

            await _log.WriteInfo("AssetCoinholderIndexesCommandsQueueConsumer", "UpdateCoinholersIndex", context.ToJson(), "Done");
        }

        public void Start()
        {
            _queueReader.Start();
            _log.WriteInfo("CoinholderIndexesCommandsQueueConsumer", "Start", null,
                $"Started:{_queueReader.GetComponentName()}");
        }
    }
}
