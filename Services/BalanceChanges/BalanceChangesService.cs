using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Core.AssetBlockChanges.Mongo;
using Core.Settings;
using NBitcoin;
using Providers;
using Providers.Helpers;
using Services.MainChain;

namespace Services.BalanceChanges
{
    public class BalanceChangesService
    {
        private readonly IAssetBalanceChangesRepository _balanceChangesRepository;
        private readonly IndexerClientFactory _indexerClient;
        private readonly MainChainService _mainChainService;
        private readonly ILog _log;
        private readonly Network _network;


        public BalanceChangesService(
            IAssetBalanceChangesRepository balanceChangesRepository,
            IndexerClientFactory indexerClient,
            MainChainService mainChainService, 
            ILog log,
            BaseSettings baseSettings)
        {
            _balanceChangesRepository = balanceChangesRepository;
            _indexerClient = indexerClient;
            _mainChainService = mainChainService;
            _log = log;
            _network = baseSettings.UsedNetwork();
        }

        public async Task<SaveAddressResult> SaveAddressChangesAsync(int fromBlockHeight, int toBlockHeight, BitcoinAddress[] addresses)
        {
            var tasksToAwait = new List<Task>();
            var mainChain = await _mainChainService.GetMainChainAsync();

            var changedAssetIds = new List<string>();
            foreach (var coloredAddress in addresses.Select(p=> new BitcoinColoredAddress(p).ToString()).Distinct())
            {
                var balanceId = BalanceIdHelper.Parse(coloredAddress, _network);

                var changesTask = _indexerClient.GetIndexerClient().GetConfirmedBalanceChangesAsync(balanceId, mainChain, fromBlockHeight, toBlockHeight).ContinueWith(
                    async task =>
                    {
                        try
                        {
                            var coloredChanges = task.Result
                            .SelectMany(p => p.GetColoredChanges(_network))
                            .Select(p => AssetBalanceChanges.Create(p.AssetId,
                                   p.Quantity,
                                   p.BlockHash,
                                   mainChain.GetBlock(uint256.Parse(p.BlockHash)).Height,
                                   p.TransactionHash))
                            .ToList();

                            await _balanceChangesRepository.AddAsync(coloredAddress, coloredChanges);

                            var assetIds = coloredChanges.Select(p => p.AssetId);
                            changedAssetIds.AddRange(assetIds);
                        }
                        catch (Exception e)
                        {
                            await _log.WriteFatalError("BalanceChangesService", "SaveAddressChangesAsync", coloredAddress.ToString(), e);
                            throw;
                        }
                    });

                tasksToAwait.Add(changesTask.Unwrap());
            }

            await Task.WhenAll(tasksToAwait);

            return new SaveAddressResult
            {
                ChangedAssetIds = changedAssetIds.Distinct().ToList()
            };
        }
    }

    public class SaveAddressResult
    {
        public IEnumerable<string> ChangedAssetIds { get; set; } 
    }
}
