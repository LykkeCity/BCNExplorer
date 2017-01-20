using System;
using System.Threading.Tasks;
using AzureRepositories.AssetCoinHolders;
using Common;
using Common.Log;
using Core.AssetBlockChanges.Mongo;
using Microsoft.Azure.WebJobs;
using Services.MainChain;

namespace AssetCoinHoldersScanner.TimerFunctions
{
    public class ParseBlocksFunctions
    {
        private readonly ILog _log;
        private readonly AssetChangesParseBlockCommandProducer _parseBlockCommandProducer;
        private readonly IAssetBalanceChangesRepository _parsedBlockRepository;
        private readonly MainChainService _mainChainService;

        public ParseBlocksFunctions(ILog log,
            AssetChangesParseBlockCommandProducer parseBlockCommandProducer,
            IAssetBalanceChangesRepository parsedBlockRepository, 
            MainChainService mainChainService)
        {
            _log = log;
            _parseBlockCommandProducer = parseBlockCommandProducer;
            _parsedBlockRepository = parsedBlockRepository;
            _mainChainService = mainChainService;
        }

        public async Task ParseLastBlock([TimerTrigger("00:05:00", RunOnStartup = true)] TimerInfo timer)
        {
            try
            {
                await _log.WriteInfo("ParseBlocksFunctions", "ParseLastBlock", null, "Started");

                var mainChain = await _mainChainService.GetMainChainAsync();

                var lastParsedBlockHeight = await _parsedBlockRepository.GetLastParsedBlockHeightAsync();
                // to put notconfirmed tx-s (at last parse block iteration) in prev block. Now this tx have to be confirmed
                var startParseBlock = lastParsedBlockHeight != 0 ? (lastParsedBlockHeight - 6) : 0;

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
