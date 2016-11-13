using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AzureRepositories;
using AzureRepositories.AssetCoinHolders;
using AzureStorage.Queue;
using Common;
using Common.Log;
using Core.Settings;
using JobsCommon;
using Providers;
using Providers.Helpers;

namespace AssetCoinHoldersScanner.QueueHandlers
{
    public class ParseBalanceChangesCommandQueueConsumer : IStarter
    {
        private readonly IParseBlockQueueReader _queueReader;
        private readonly ILog _log;
        private readonly IndexerClientFactory _indexerClient;
        private readonly MainChainRepository _mainChainRepository;
        private readonly BaseSettings _baseSettings;
        private readonly BalanceChangesService _balanceChangesService;

        public ParseBalanceChangesCommandQueueConsumer(ILog log, 
            IParseBlockQueueReader queueReader,
            IndexerClientFactory indexerClient, 
            MainChainRepository mainChainRepository, 
            BaseSettings baseSettings, 
            BalanceChangesService balanceChangesService)
        {
            _log = log;
            _queueReader = queueReader;
            _indexerClient = indexerClient;
            _mainChainRepository = mainChainRepository;
            _baseSettings = baseSettings;
            _balanceChangesService = balanceChangesService;

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
            await _log.WriteInfo("ParseBalanceChangesCommandQueueConsumer", "ParseBlock", context.ToJson(), "Started");

            try
            {
                var st = new Stopwatch();
                st.Start();
                var mainChain = await _mainChainRepository.GetMainChainAsync();

                var block = _indexerClient.GetIndexerClient().GetBlock(mainChain.GetBlock(context.BlockHeight).HashBlock);
                var addressesToTrack = (await block.GetAddressesWithColoredMarkerAsync(_baseSettings.UsedNetwork(), _indexerClient.GetIndexerClient())).ToArray();

                await _balanceChangesService.SaveAddressChangesAsync(context.BlockHeight, context.BlockHeight, addressesToTrack);

                await _log.WriteInfo("ParseBalanceChangesCommandQueueConsumer", "ParseBlock", context.ToJson(), $"Done {st.Elapsed.ToString("g")}. Addr to track {addressesToTrack.Length}");
                //await _log.WriteInfo("ParseBalanceChangesCommandQueueConsumer", "ParseBlock", context.ToJson(), "Done");
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
