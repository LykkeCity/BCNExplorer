using System;
using System.Threading.Tasks;
using AzureRepositories.Asset;
using Common;
using Common.Log;
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

        public ParseBlocksFunctions(ILog log, ParseBlockCommandProducer parseBlockCommandProducer, IndexerClient indexerClient)
        {
            _log = log;
            _parseBlockCommandProducer = parseBlockCommandProducer;
            _indexerClient = indexerClient;
        }

        public async Task ParseLast([TimerTrigger("00:10:00", RunOnStartup = true)] TimerInfo timer)
        {
            await _log.WriteInfo("ParseBlocksFunctions", "ParseLast", null, "Started");

            var blockPtr =
                _indexerClient.GetBestBlock().Header;

            try
            {
                while (blockPtr != null)
                {
                    await _parseBlockCommandProducer.CreateParseBlockCommand(blockPtr.GetBlockId());
                    blockPtr = _indexerClient.GetBlock(blockPtr.HashPrevBlock).Header;
                }

                await _log.WriteInfo("ParseBlocksFunctions", "ParseLast", null, "Done");
            }
            catch (Exception e)
            {
                await _log.WriteError("ParseBlocksFunctions", "ParseLast", (new {blockHash = blockPtr.GetBlockId() }).ToJson(), e);
                throw;
            }
        }
    }
}
