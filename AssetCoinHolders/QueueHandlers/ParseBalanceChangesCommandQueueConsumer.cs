using System;
using System.Linq;
using System.Threading.Tasks;
using AzureRepositories;
using AzureRepositories.AssetCoinHolders;
using AzureRepositories.AssetDefinition;
using AzureStorage.Queue;
using Common;
using Common.Log;
using Core.Asset;
using NBitcoin;
using NBitcoin.Indexer;
using NBitcoin.OpenAsset;
using Providers.Helpers;

namespace AssetCoinHoldersScanner.QueueHandlers
{
    public class ParseBalanceChangesCommandQueueConsumer : IStarter
    {
        private readonly IParseBlockQueueReader _queueReader;
        private readonly ILog _log;
        private readonly IndexerClient _indexerClient;

        public ParseBalanceChangesCommandQueueConsumer(ILog log, 
            IParseBlockQueueReader queueReader, 
            IndexerClient indexerClient)
        {
            _log = log;
            _queueReader = queueReader;
            _indexerClient = indexerClient;

            _queueReader.RegisterPreHandler(async data =>
            {
                if (data == null)
                {
                    await _log.WriteInfo("ParseBalanceChangesCommandQueueConsumer", "InitQueues", null, "Queue had unknown message");
                    return false;
                }
                return true;
            });

            _queueReader.RegisterHandler<QueueRequestModel<AssetChangesParseBlockContext>>(
                AssetChangesParseBlockContext.Id, itm => ParseBlock(itm.Data));
        }

        private async Task ParseBlock(AssetChangesParseBlockContext context)
        {
            try
            {
                await _log.WriteInfo("ParseBalanceChangesCommandQueueConsumer", "ParseBlock", context.ToJson(), "Done");
                
            }
            catch (Exception e)
            {
                await _log.WriteError("ParseBalanceChangesCommandQueueConsumer", "ParseBlock", context.ToJson(), e);
            }
        }
        

        public void Start()
        {
            _queueReader.Start();
            _log.WriteInfo("ParseBalanceChangesCommandQueueConsumer", "Start", null,
                $"Started:{_queueReader.GetComponentName()}");
        }
    }
}
