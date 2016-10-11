using System;
using System.Linq;
using System.Threading.Tasks;
using AzureRepositories.Asset;
using Common.Log;
using JobsCommon;
using Microsoft.Azure.WebJobs;
using NBitcoin.Indexer;
using Providers.BlockChainReader;
using Providers.Helpers;
using Providers.Providers.Ninja;

namespace AssetScanner.Functions
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
            var lastBlock = _indexerClient.GetBestBlock();
            await _parseBlockCommandProducer.CreateParseBlockCommand(lastBlock.BlockId.ToString());


            //var txId = "0c0a21b640b1011ed397138d76b81375d35c10e419b52fc887677634dd7bcd4f";
            //var transaction = await _ninjaTransactionProvider.GetAsync(txId);
            //var definitionUrl = TransactionHelper.TryGetAssetDefinitionUrl(transaction.Hex);
            //if (definitionUrl != null)
            //{
            //    await _assetDataCommandProducer.CreateUpdateAssetDataCommand(definitionUrl.AbsoluteUri);
            //}



            //var lastBlockHeader = await _ninjaBlockProvider.GetLastBlockAsync();

            //var lastBlock = await _ninjaBlockProvider.GetAsync(lastBlockHeader.Hash);


            //foreach (var transactionId in lastBlock.TransactionIds.Take(1))
            //{
            //    var transaction = await _ninjaTransactionProvider.GetAsync("13221b901f1d561f50316cf601435da95094f8d93654e48889c0e8c12e5c1e84");
            //    var definitionUrl = AssetHelper.TryGetAssetDefinitionUrl(transaction.Hex);
            //    if (definitionUrl != null)
            //    {
            //        await _assetDataCommandProducer.CreateUpdateAssetDataCommand(definitionUrl.AbsoluteUri);
            //    }
            //}
        }
    }
}
