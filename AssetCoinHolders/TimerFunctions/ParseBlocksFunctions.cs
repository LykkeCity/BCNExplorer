using System;
using System.Threading.Tasks;
using AzureRepositories.AssetCoinHolders;
using Common;
using Common.Log;
using Core.AssetBlockChanges.Mongo;
using Core.Block;
using Microsoft.Azure.WebJobs;
using Providers.Helpers;
using Services.BalanceChanges;
using Services.MainChain;

namespace AssetCoinHoldersScanner.TimerFunctions
{
    public class ParseBlocksFunctions
    {
        private readonly ILog _log;
        private readonly AssetChangesParseBlockCommandProducer _parseBlockCommandProducer;
        private readonly IAssetBalanceChangesRepository _parsedBlockRepository;
        private readonly MainChainService _mainChainService;
        private readonly BalanceChangesService _balanceChangesService;

        public ParseBlocksFunctions(ILog log,
            AssetChangesParseBlockCommandProducer parseBlockCommandProducer,
            IAssetBalanceChangesRepository parsedBlockRepository, 
            MainChainService mainChainService, BalanceChangesService balanceChangesService)
        {
            _log = log;
            _parseBlockCommandProducer = parseBlockCommandProducer;
            _parsedBlockRepository = parsedBlockRepository;
            _mainChainService = mainChainService;
            _balanceChangesService = balanceChangesService;
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


        public async Task RemoveForkBlocks([TimerTrigger("01:00:00", RunOnStartup = true)] TimerInfo timer)
        {
            try
            {
                await _log.WriteInfo("ParseBlocksFunctions", "RemoveForkBlocks", null, "Started");

                var mainChain = await _mainChainService.GetMainChainAsync();

                var removeFrom = mainChain.GetClosestToTimeBlock(DateTime.UtcNow.AddHours(-2));

                await _balanceChangesService.RemoveForksAsync(removeFrom.Height);

                await _log.WriteInfo("ParseBlocksFunctions", "RemoveForkBlocks", null, "Done");
            }
            catch (Exception e)
            {
                await
                    _log.WriteError("ParseBlocksFunctions", "RemoveForkBlocks", null, e);
            }
        }
    }
}
