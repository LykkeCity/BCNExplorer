using System;
using System.Threading.Tasks;
using AzureRepositories;
using AzureRepositories.AssetDefinition;
using Common;
using Common.Log;
using Core.Asset;
using Microsoft.Azure.WebJobs;
using NBitcoin;
using NBitcoin.Indexer;
using Providers;
using Providers.Helpers;

namespace AssetDefinitionScanner.TimerFunctions
{
    public class ParseBlocksFunctions
    {
        private readonly ILog _log;
        private readonly AssetDefinitionParseBlockCommandProducer _parseBlockCommandProducer;
        private readonly IndexerClientFactory _indexerClient;
        private readonly IAssetDefinitionParsedBlockRepository _assetDefinitionParsedBlockRepository;

        public ParseBlocksFunctions(ILog log, AssetDefinitionParseBlockCommandProducer parseBlockCommandProducer, IndexerClientFactory indexerClient, IAssetDefinitionParsedBlockRepository assetDefinitionParsedBlockRepository)
        {
            _log = log;
            _parseBlockCommandProducer = parseBlockCommandProducer;
            _indexerClient = indexerClient;
            _assetDefinitionParsedBlockRepository = assetDefinitionParsedBlockRepository;
        }

        public async Task ParseLast([TimerTrigger("00:10:00", RunOnStartup = true)] TimerInfo timer)
        {
            BlockHeader blockPtr = null;

            try
            {
                await _log.WriteInfo("ParseBlocksFunctions", "ParseLast", null, "Started");
                var client = _indexerClient.GetIndexerClient();
                blockPtr = client.GetBestBlock().Header;
                while (blockPtr != null && !(await _assetDefinitionParsedBlockRepository.IsBlockExistsAsync(AssetDefinitionParsedBlock.Create(blockPtr.GetBlockId()))))
                {
                    await _parseBlockCommandProducer.CreateParseBlockCommand(blockPtr.GetBlockId());

                    blockPtr = client.GetBlock(blockPtr.HashPrevBlock).Header;
                }

                await _log.WriteInfo("ParseBlocksFunctions", "ParseLast", null, "Done");
            }
            catch (Exception e)
            {
                await _log.WriteError("ParseBlocksFunctions", "ParseLast", (new { blockHash = blockPtr?.GetBlockId() }).ToJson(), e);
                throw;
            }
        }
    }
}
