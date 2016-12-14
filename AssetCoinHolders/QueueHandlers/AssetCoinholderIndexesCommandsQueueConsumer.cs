using System;
using System.Linq;
using System.Threading.Tasks;
using AzureRepositories.AssetCoinHolders;
using AzureRepositories.QueueReaders;
using AzureStorage.Queue;
using Common;
using Common.Log;
using Core.Asset;
using Core.AssetBlockChanges.Mongo;
using Core.Transaction;
using Providers.Helpers;
using Services.MainChain;

namespace AssetCoinHoldersScanner.QueueHandlers
{
    public class AssetCoinholderIndexesCommandsQueueConsumer : IStarter
    {
        private readonly ILog _log;
        private readonly ICoinholderIndexesQueueReader _queueReader;
        private readonly IAssetCoinholdersIndexRepository _assetCoinholdersIndexRepository;
        private readonly IAssetBalanceChangesRepository _balanceChangesRepository;
        private readonly IAssetService _assetService;
        private readonly ITransactionService _transactionService;
        private readonly MainChainRepository _mainChainRepository;

        public AssetCoinholderIndexesCommandsQueueConsumer(ILog log, 
            ICoinholderIndexesQueueReader queueReader,
            IAssetCoinholdersIndexRepository assetCoinholdersIndexRepository, 
            IAssetBalanceChangesRepository balanceChangesRepository, 
            IAssetService assetService, 
            ITransactionService transactionService, MainChainRepository mainChainRepository)
        {
            _log = log;
            _queueReader = queueReader;
            _assetCoinholdersIndexRepository = assetCoinholdersIndexRepository;
            _balanceChangesRepository = balanceChangesRepository;
            _assetService = assetService;
            _transactionService = transactionService;
            _mainChainRepository = mainChainRepository;

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
            try
            {
                await
                    _log.WriteInfo("AssetCoinholderIndexesCommandsQueueConsumer", "UpdateCoinholersIndex",
                        context.ToJson(), "Started");

                var asset = await _assetService.GetAssetAsync(context.AssetId);
                if (asset != null)
                {
                    var mainChain = await _mainChainRepository.GetMainChainAsync();

                    var balanceSummary = _balanceChangesRepository.GetSummaryAsync(asset.AssetIds.ToArray());
                    var blocksWithChanges = _balanceChangesRepository.GetBlocksWithChanges(asset.AssetIds);
                    var allTxs = _balanceChangesRepository.GetTransactionsAsync(asset.AssetIds);
                    var monthAgoBlock = mainChain.GetClosestToTimeBlock(DateTime.Now.AddDays(-30));
                    var lastMonthTxs = _balanceChangesRepository.GetTransactionsAsync(asset.AssetIds,
                        monthAgoBlock?.Height);

                    var lastTxDate = _balanceChangesRepository.GetLatestTxAsync(asset.AssetIds)
                        .ContinueWith(async p => (await _transactionService.GetAsync(p.Result?.Hash))?.Block?.Time);

                    await Task.WhenAll(balanceSummary, blocksWithChanges, allTxs, lastTxDate.Unwrap(), lastMonthTxs);

                    await _assetCoinholdersIndexRepository.InserOrReplaceAsync(
                            AssetCoinholdersIndex.Create(balanceSummary.Result, 
                                blocksWithChanges.Result, 
                                allTxs.Result.Count(), lastMonthTxs.Result.Count(), lastTxDate.Unwrap().Result));
                }

                await
                    _log.WriteInfo("AssetCoinholderIndexesCommandsQueueConsumer", "UpdateCoinholersIndex",
                        context.ToJson(), "Done");
            }
            catch (Exception e)
            {
                await  _log.WriteError("AssetCoinholderIndexesCommandsQueueConsumer", "UpdateCoinholersIndex",
                   context.ToJson(), e);
            }
        }

        public void Start()
        {
            _queueReader.Start();
            _log.WriteInfo("CoinholderIndexesCommandsQueueConsumer", "Start", null,
                $"Started:{_queueReader.GetComponentName()}");
        }
    }
}
