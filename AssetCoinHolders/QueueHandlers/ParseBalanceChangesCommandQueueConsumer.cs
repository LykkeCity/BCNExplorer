﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AzureRepositories.AssetCoinHolders;
using AzureRepositories.QueueReaders;
using AzureStorage.Queue;
using Common;
using Common.Log;
using Core.Settings;
using Providers;
using Providers.Helpers;
using Services.BalanceChanges;
using Services.MainChain;

namespace AssetCoinHoldersScanner.QueueHandlers
{
    public class ParseBalanceChangesCommandQueueConsumer : IStarter
    {
        private readonly IParseBlockQueueReader _queueReader;
        private readonly ILog _log;
        private readonly IndexerClientFactory _indexerClient;
        private readonly MainChainService _mainChainService;
        private readonly BaseSettings _baseSettings;
        private readonly BalanceChangesService _balanceChangesService;
        private readonly AssetChangesParseBlockCommandProducer _parseBlockCommandProducer;

        private readonly AssetCoinholdersIndexesCommandProducer _assetCoinholdersIndexesCommandProducer;

        private const int attemptCount = 10;

        public ParseBalanceChangesCommandQueueConsumer(ILog log, 
            IParseBlockQueueReader queueReader,
            IndexerClientFactory indexerClient, 
            MainChainService mainChainService, 
            BaseSettings baseSettings, 
            BalanceChangesService balanceChangesService, 
            AssetChangesParseBlockCommandProducer parseBlockCommandProducer, 
            AssetCoinholdersIndexesCommandProducer assetCoinholdersIndexesCommandProducer)
        {
            _log = log;
            _queueReader = queueReader;
            _indexerClient = indexerClient;
            _mainChainService = mainChainService;
            _baseSettings = baseSettings;
            _balanceChangesService = balanceChangesService;
            _parseBlockCommandProducer = parseBlockCommandProducer;
            _assetCoinholdersIndexesCommandProducer = assetCoinholdersIndexesCommandProducer;

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
                var mainChain = await _mainChainService.GetMainChainAsync();

                var block = _indexerClient.GetIndexerClient().GetBlock(mainChain.GetBlock(context.BlockHeight).HashBlock);
                var addressesToTrack = (await block.GetAddressesWithColoredMarkerAsync(_baseSettings.UsedNetwork(), _indexerClient.GetIndexerClient())).ToArray();

                var saveResult = await _balanceChangesService.SaveAddressChangesAsync(context.BlockHeight, context.BlockHeight, addressesToTrack);

                await _assetCoinholdersIndexesCommandProducer.CreateAssetCoinholdersUpdateIndexCommand(saveResult.ChangedAssetIds.ToArray());

                await _log.WriteInfo("ParseBalanceChangesCommandQueueConsumer", "ParseBlock", context.ToJson(), $"Done. Addr to track {addressesToTrack.Length}. SaveResult {saveResult.ToJson()}");
            }
            catch (Exception e)
            {
                await _log.WriteError("ParseBalanceChangesCommandQueueConsumer", "ParseBlock", context.ToJson(), e);
                if (context.Attempt <= attemptCount)
                {
                    await _parseBlockCommandProducer.CreateParseBlockCommand(context.BlockHeight, ++context.Attempt);
                }
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
