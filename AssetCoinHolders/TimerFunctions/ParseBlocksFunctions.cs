using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AzureRepositories.AssetCoinHolders;
using Common;
using Common.Log;
using Core.Asset;
using JobsCommon;
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
        private readonly MainChainRepository _mainChainRepository;

        public ParseBlocksFunctions(ILog log, 
            AssetChangesParseBlockCommandProducer parseBlockCommandProducer, 
            IndexerClient indexerClient, 
            IAssetChangesParsedBlockRepository parsedBlockRepository, MainChainRepository mainChainRepository)
        {
            _log = log;
            _parseBlockCommandProducer = parseBlockCommandProducer;
            _indexerClient = indexerClient;
            _parsedBlockRepository = parsedBlockRepository;
            _mainChainRepository = mainChainRepository;
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

        public async Task TestRetrieveChanges([TimerTrigger("00:59:00", RunOnStartup = true)] TimerInfo timer)
        {
            var st = new Stopwatch();
            var mainchain = await _mainChainRepository.GetMainChainAsync();
            var coloredAddresses = _indexerClient.GetBlock(uint256.Parse("0000000000000000029559b0665cacb4470eda0696a69744263e82e7e4d0f27d")).GetAddressesWithColoredMarker(Network.Main);
            var checkTasks = new List<Task>();

            await _log.WriteInfo("TestRetrieveChanges", "TestRetrieveChanges", st.Elapsed.ToString("g"), "Started");
            st.Start();
            var semaphore = new SemaphoreSlim(1000);
            foreach (var address in coloredAddresses)
            {
                var balanceId = BalanceIdHelper.Parse(address.ToString(), Network.Main);
                //checkTasks.Add(_indexerClient.GetConfirmedBalanceChangesAsync(balanceId, mainchain, semaphore));p
            }

            await Task.WhenAll(checkTasks);

            st.Stop();

            await _log.WriteInfo("TestRetrieveChanges", "TestRetrieveChanges", st.Elapsed.ToString("g"), "Finished");
        }
    }
}
