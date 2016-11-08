using System;
using System.Linq;
using System.Threading.Tasks;
using AzureRepositories;
using AzureRepositories.AssetCoinHolders;
using AzureRepositories.AssetDefinition;
using AzureStorage.Queue;
using Common;
using Common.Log;
using Core.Asset;
using NBitcoin;
using NBitcoin.Indexer;
using NBitcoin.OpenAsset;
using Providers.Helpers;

namespace AssetCoinHoldersScanner.QueueHandlers
{
    public class ParseBalanceChangesCommandQueueConsumer : IStarter
    {
        private readonly IParseBlockQueueReader _queueReader;
        private readonly ILog _log;
        private readonly IndexerClient _indexerClient;
        private readonly IAssetDefinitionParsedBlockRepository _assetDefinitionParsedBlockRepository;

        public ParseBalanceChangesCommandQueueConsumer(ILog log, 
            IParseBlockQueueReader queueReader, 
            IndexerClient indexerClient, 
            IAssetDefinitionParsedBlockRepository assetDefinitionParsedBlockRepository)
        {
            _log = log;
            _queueReader = queueReader;
            _indexerClient = indexerClient;
            _assetDefinitionParsedBlockRepository = assetDefinitionParsedBlockRepository;

            _queueReader.RegisterPreHandler(async data =>
            {
                if (data == null)
                {
                    await _log.WriteInfo("ParseBalanceChangesCommandQueueConsumer", "InitQueues", null, "Queue had unknown message");
                    return false;
                }
                return true;
            });

            _queueReader.RegisterHandler<QueueRequestModel<AssetChangesParseBlockContext>>(
                AssetChangesParseBlockContext.Id, itm => ParseBlock(itm.Data));
        }

        private async Task ParseBlock(AssetChangesParseBlockContext context)
        {
            try
            {
                var block = _indexerClient.GetBlock(context.BlockHash);



                await _log.WriteInfo("ParseBalanceChangesCommandQueueConsumer", "ParseBlock", context.ToJson(), "Done");

                await _assetDefinitionParsedBlockRepository.AddBlockAsync(AssetDefinitionParsedBlock.Create(context.BlockHash));
            }
            catch (Exception e)
            {
                await _log.WriteError("ParseBalanceChangesCommandQueueConsumer", "ParseBlock", context.ToJson(), e);
            }
        }
        

        public void Start()
        {
            _queueReader.Start();
            _log.WriteInfo("ParseBalanceChangesCommandQueueConsumer", "Start", null,
                $"Started:{_queueReader.GetComponentName()}");
        }
    }
}
