﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AzureRepositories.AssetCoinHolders;
using Common;
using Common.Log;
using Core.Asset;
using Core.AssetBlockChanges.Mongo;
using JobsCommon;
using Microsoft.Azure.WebJobs;
using NBitcoin;
using NBitcoin.Indexer;
using Providers;
using Providers.Helpers;

namespace AssetCoinHoldersScanner.TimerFunctions
{
    public class ParseBlocksFunctions
    {
        private readonly ILog _log;
        private readonly AssetChangesParseBlockCommandProducer _parseBlockCommandProducer;
        private readonly IAssetBalanceChangesRepository _parsedBlockRepository;
        private readonly MainChainRepository _mainChainRepository;

        public ParseBlocksFunctions(ILog log,
            AssetChangesParseBlockCommandProducer parseBlockCommandProducer,
            IAssetBalanceChangesRepository parsedBlockRepository, 
            MainChainRepository mainChainRepository)
        {
            _log = log;
            _parseBlockCommandProducer = parseBlockCommandProducer;
            _parsedBlockRepository = parsedBlockRepository;
            _mainChainRepository = mainChainRepository;
        }

        public async Task ParseLastBlock([TimerTrigger("00:05:00", RunOnStartup = true)] TimerInfo timer)
        {
            try
            {
                await _log.WriteInfo("ParseBlocksFunctions", "ParseLastBlock", null, "Started");

                var mainChain = await _mainChainRepository.GetMainChainAsync();

                var lastParsedBlockHeight = await _parsedBlockRepository.GetLastParsedBlockHeightAsync();
                // to put notconfirmed tx-s (at last parse block iteration) in prev block. Now this tx have to be confirmed
                var startParseBlock = lastParsedBlockHeight != 0 ? (lastParsedBlockHeight - 2) : 0;

                for (var i = startParseBlock; i <= mainChain.Tip.Height; i++)
                {
                    await _parseBlockCommandProducer.CreateParseBlockCommand(i);
                    await _log.WriteInfo("ParseBlocksFunctions", "ParseLastBlock", i.ToString(), "Add parse block command done");
                }

                await _log.WriteInfo("ParseBlocksFunctions", "ParseLastBlock", new { lastParsedBlockHeight }.ToJson(), "Done");
            }
            catch (Exception e)
            {
                await
                    _log.WriteError("ParseBlocksFunctions", "ParseLastBlock", null, e);
            }
        }

        //    public async Task TestRetrieveChanges([TimerTrigger("00:59:00", RunOnStartup = true)] TimerInfo timer)
        //    {
        //        var st = new Stopwatch();
        //        var mainchain = await _mainChainRepository.GetMainChainAsync();
        //        var coloredAddresses = _indexerClient.GetBlock(uint256.Parse("0000000000000000029559b0665cacb4470eda0696a69744263e82e7e4d0f27d")).GetAddressesWithColoredMarker(Network.Main);
        //        var checkTasks = new List<Task>();

        //        await _log.WriteInfo("TestRetrieveChanges", "TestRetrieveChanges", st.Elapsed.ToString("g"), "Started");
        //        st.Start();
        //        var semaphore = new SemaphoreSlim(1000);
        //        foreach (var address in coloredAddresses)
        //        {
        //            var balanceId = BalanceIdHelper.Parse(address.ToString(), Network.Main);
        //            //checkTasks.Add(_indexerClient.GetConfirmedBalanceChangesAsync(balanceId, mainchain, semaphore));p
        //        }

        //        await Task.WhenAll(checkTasks);

        //        st.Stop();

        //        await _log.WriteInfo("TestRetrieveChanges", "TestRetrieveChanges", st.Elapsed.ToString("g"), "Finished");
        //    }
        //}
    }
}
