using System;
using System.Linq;
using System.Threading.Tasks;
using AzureRepositories.Asset;
using Common.Log;
using JobsCommon;
using Microsoft.Azure.WebJobs;
using Providers.BlockChainReader;
using Providers.Helpers;
using Providers.Providers.Ninja;

namespace AssetScanner.Functions
{
    public class AssetCreatorFunctions
    {
        private readonly ILog _log;
        private readonly NinjaBlockProvider _ninjaBlockProvider;
        private readonly NinjaTransactionProvider _ninjaTransactionProvider;
        private readonly AssetDataCommandProducer _assetDataCommandProducer;

        public AssetCreatorFunctions(ILog log, NinjaBlockProvider ninjaBlockProvider, 
            NinjaTransactionProvider ninjaTransactionProvider, AssetDataCommandProducer assetDataCommandProducer)
        {
            _log = log;
            _ninjaBlockProvider = ninjaBlockProvider;
            _ninjaTransactionProvider = ninjaTransactionProvider;
            _assetDataCommandProducer = assetDataCommandProducer;
        }

        public async Task ParseLastBlock([TimerTrigger("00:10:00", RunOnStartup = true)] TimerInfo timer)
        {

            var txId = "f6c78f15f6c540101c153397288141e7ef3f1a34562d24810e558d87793dacc6";
            var transaction = await _ninjaTransactionProvider.GetAsync(txId);
            var definitionUrl = AssetHelper.TryGetAssetDefinitionUrl(transaction.Hex);
            if (definitionUrl != null)
            {
                await _assetDataCommandProducer.CreateUpdateAssetDataCommand(definitionUrl.AbsoluteUri);
            }



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
