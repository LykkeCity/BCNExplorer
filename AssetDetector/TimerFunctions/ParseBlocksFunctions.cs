using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureRepositories.Asset;
using Common;
using Common.Log;
using Core.Asset;
using Microsoft.Azure.WebJobs;
using NBitcoin;
using NBitcoin.Indexer;
using Providers.Helpers;

namespace AssetScanner.TimerFunctions
{
    public class ParseBlocksFunctions
    {
        private readonly ILog _log;
        private readonly ParseBlockCommandProducer _parseBlockCommandProducer;
        private readonly IndexerClient _indexerClient;
        private readonly IAssetParsedBlockRepository _assetParsedBlockRepository;

        public ParseBlocksFunctions(ILog log, ParseBlockCommandProducer parseBlockCommandProducer, IndexerClient indexerClient, IAssetParsedBlockRepository assetParsedBlockRepository)
        {
            _log = log;
            _parseBlockCommandProducer = parseBlockCommandProducer;
            _indexerClient = indexerClient;
            _assetParsedBlockRepository = assetParsedBlockRepository;
        }

        public async Task ParseLast([TimerTrigger("00:10:00", RunOnStartup = true)] TimerInfo timer)
        {
            BlockHeader blockPtr = null;

            try
            {
                blockPtr = _indexerClient.GetBestBlock().Header;
                while (blockPtr != null && !(await _assetParsedBlockRepository.IsBlockExistsAsync(AssetParsedBlock.Create(blockPtr.GetBlockId()))))
                {
                    await _parseBlockCommandProducer.CreateParseBlockCommand(blockPtr.GetBlockId());

                    blockPtr = _indexerClient.GetBlock(blockPtr.HashPrevBlock).Header;
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
