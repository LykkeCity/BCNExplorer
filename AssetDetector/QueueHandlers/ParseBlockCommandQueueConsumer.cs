using System;
using System.Linq;
using System.Threading.Tasks;
using AzureRepositories;
using AzureRepositories.AssetDefinition;
using AzureStorage.Queue;
using Common;
using Common.Log;
using Core.Asset;
using NBitcoin.Indexer;
using NBitcoin.OpenAsset;
using Providers.Helpers;

namespace AssetDefinitionScanner.QueueHandlers
{
    public class ParseBlockCommandQueueConsumer : IStarter
    {
        private readonly IParseBlockQueueReader _queueReader;
        private readonly ILog _log;
        private readonly IndexerClient _indexerClient;
        private readonly IAssetDefinitionParsedBlockRepository _assetDefinitionParsedBlockRepository;
        private readonly AssetDataCommandProducer _assetDataCommandProducer;

        public ParseBlockCommandQueueConsumer(ILog log, 
            IParseBlockQueueReader queueReader, 
            IndexerClient indexerClient, 
            AssetDataCommandProducer assetDataCommandProducer)
        {
            _log = log;
            _queueReader = queueReader;
            _indexerClient = indexerClient;
            _assetDataCommandProducer = assetDataCommandProducer;

            _queueReader.RegisterPreHandler(async data =>
            {
                if (data == null)
                {
                    await _log.WriteInfo("ParseBlockCommandQueueConsumer", "InitQueues", null, "Queue had unknown message");
                    return false;
                }
                return true;
            });

            _queueReader.RegisterHandler<QueueRequestModel<AssetDefinitionParseBlockContext>>(
                AssetDefinitionParseBlockContext.Id, itm => ParseBlock(itm.Data));
        }

        private async Task ParseBlock(AssetDefinitionParseBlockContext context)
        {
            try
            {
                var block = _indexerClient.GetBlock(context.BlockHash);

                foreach (var transaction in block.Transactions.Where(p => p.HasValidColoredMarker()))
                {
                    var assetDefUrl = transaction.TryGetAssetDefinitionUrl();

                    if (assetDefUrl != null)
                    {
                        await _assetDataCommandProducer.CreateUpdateAssetDataCommand(assetDefUrl.AbsoluteUri);
                    }
                }

                await _log.WriteInfo("ParseBlockCommandQueueConsumer", "ParseBlock", context.ToJson(), "Done");

                await _assetDefinitionParsedBlockRepository.AddBlockAsync(AssetDefinitionParsedBlock.Create(context.BlockHash));
            }
            catch (Exception e)
            {
                await _log.WriteError("ParseBlockCommandQueueConsumer", "ParseBlock", context.ToJson(), e);
            }
        }
        

        public void Start()
        {
            _queueReader.Start();
            _log.WriteInfo("ParseBlockCommandQueueConsumer", "Start", null,
                $"Started:{_queueReader.GetComponentName()}");
        }
    }
}
