using System;
using System.Threading.Tasks;
using AzureRepositories.AssetCoinHolders;
using Common;
using Common.Log;
using Core.Asset;
using Microsoft.Azure.WebJobs;
using NBitcoin;
using NBitcoin.Indexer;
using Providers.Helpers;

namespace AssetCoinHoldersScanner.TimerFunctions
{
    public class ParseBlocksFunctions
    {
        private readonly ILog _log;
        private readonly AssetChangesParseBlockCommandProducer _parseBlockCommandProducer;
        private readonly IndexerClient _indexerClient;
        private readonly IAssetChangesParsedBlockRepository _parsedBlockRepository;

        public ParseBlocksFunctions(ILog log, 
            AssetChangesParseBlockCommandProducer parseBlockCommandProducer, 
            IndexerClient indexerClient, 
            IAssetChangesParsedBlockRepository parsedBlockRepository)
        {
            _log = log;
            _parseBlockCommandProducer = parseBlockCommandProducer;
            _indexerClient = indexerClient;
            _parsedBlockRepository = parsedBlockRepository;
        }

        //public async Task ParseLast([TimerTrigger("00:10:00", RunOnStartup = true)] TimerInfo timer)
        //{
        //    BlockHeader blockPtr = null;

        //    try
        //    {
        //        blockPtr = _indexerClient.GetBestBlock().Header;
        //        while (blockPtr != null && !(await _parsedBlockRepository.IsBlockExistsAsync(AssetChangesParsedBlock.Create(blockPtr.GetBlockId()))))
        //        {
        //            await _parseBlockCommandProducer.CreateParseBlockCommand(blockPtr.GetBlockId());

        //            blockPtr = _indexerClient.GetBlock(blockPtr.HashPrevBlock).Header;
        //        }

        //        await _log.WriteInfo("ParseBlocksFunctions", "ParseLast", null, "Done");
        //    }
        //    catch (Exception e)
        //    {
        //        await _log.WriteError("ParseBlocksFunctions", "ParseLast", (new { blockHash = blockPtr?.GetBlockId() }).ToJson(), e);
        //        throw;
        //    }
        //}
    }
}
