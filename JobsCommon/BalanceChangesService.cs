using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Core.AssetBlockChanges;
using Core.Settings;
using NBitcoin;
using Providers;
using Providers.Helpers;
using IBlockRepository = Core.AssetBlockChanges.IBlockRepository;
using ITransactionRepository = Core.AssetBlockChanges.ITransactionRepository;

namespace JobsCommon
{
    public class BalanceChangesService
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IBlockRepository _blockRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBalanceChangesRepository _balanceChangesRepository;
        private readonly IndexerClientFactory _indexerClient;
        private readonly MainChainRepository _mainChainRepository;
        private readonly ILog _log;
        private readonly Network _network;


        public BalanceChangesService(IAddressRepository addressRepository, 
            IBlockRepository blockRepository, 
            ITransactionRepository transactionRepository, 
            IBalanceChangesRepository balanceChangesRepository,
            IndexerClientFactory indexerClient,
            MainChainRepository mainChainRepository, 
            ILog log,
            BaseSettings baseSettings)
        {
            _addressRepository = addressRepository;
            _blockRepository = blockRepository;
            _transactionRepository = transactionRepository;
            _balanceChangesRepository = balanceChangesRepository;
            _indexerClient = indexerClient;
            _mainChainRepository = mainChainRepository;
            _log = log;
            _network = baseSettings.UsedNetwork();
        }

        public async Task SaveAddressChangesAsync(int fromBlockHeight, int toBlockHeight, BitcoinAddress[] addresses)
        {
            var semaphore = new SemaphoreSlim(100);
            var tasksToAwait = new List<Task>();
            var mainChain = await _mainChainRepository.GetMainChainAsync();
            //await _addressRepository.AddAsync(addresses.Select(p => new Address {ColoredAddress = p}).ToArray());
            foreach (var address in addresses.Distinct())
            {
                var balanceId = BalanceIdHelper.Parse(address.ToString(), _network);

                var changesTask = _indexerClient.GetIndexerClient().GetConfirmedBalanceChangesAsync(balanceId, mainChain, semaphore, fromBlockHeight, toBlockHeight).ContinueWith(
                    async task =>
                    {
                        try
                        {

                            var coloredChanges = task.Result.SelectMany(p => p.GetColoredChanges(_network)).ToList();

                            var blocks =
                                coloredChanges.Select(p => p.BlockHash).Select(p => new Core.AssetBlockChanges.Block
                                {
                                    Hash = p,
                                    Height = mainChain.GetBlock(uint256.Parse(p)).Height
                                }).ToArray();

                            await _blockRepository.AddAsync(blocks);

                            var transactions = coloredChanges.Select(p => new Core.AssetBlockChanges.Transaction
                            {
                                Hash = p.TransactionHash,
                                BlockHash = p.BlockHash
                            }).ToArray();

                            await _transactionRepository.AddAsync(transactions);

                            var balanceChanges = coloredChanges.Select(p => new BalanceChange
                            {
                                AssetId = p.AssetId,
                                Change = p.Quantity,
                                TransactionHash = p.TransactionHash,
                                Address = address.ToString(),
                                BlockHash = p.BlockHash
                            }).ToArray();

                            await _balanceChangesRepository.AddAsync(address.ToString(), balanceChanges);
                        }
                        catch (Exception e)
                        {
                            await _log.WriteFatalError("BalanceChangesService", "SaveAddressChangesAsync", address.ToString(), e);
                        }
                    });

                tasksToAwait.Add(changesTask.Unwrap());
            }

            await Task.WhenAll(tasksToAwait);
        }
    }
}
