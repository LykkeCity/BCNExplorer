using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Core.AssetBlockChanges;
using JobsCommon;
using NBitcoin;
using NBitcoin.Indexer;
using Providers.Helpers;
using IBlockRepository = Core.AssetBlockChanges.IBlockRepository;
using ITransactionRepository = Core.AssetBlockChanges.ITransactionRepository;

namespace Services.Binders
{
    public class BalanceChangesService
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IBlockRepository _blockRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBalanceChangesRepository _balanceChangesRepository;
        private readonly IndexerClient _indexerClient;
        private readonly MainChainRepository _mainChainRepository;
        private readonly ILog _log;


        public BalanceChangesService(IAddressRepository addressRepository, 
            IBlockRepository blockRepository, 
            ITransactionRepository transactionRepository, 
            IBalanceChangesRepository balanceChangesRepository, 
            IndexerClient indexerClient,
            MainChainRepository mainChainRepository, ILog log)
        {
            _addressRepository = addressRepository;
            _blockRepository = blockRepository;
            _transactionRepository = transactionRepository;
            _balanceChangesRepository = balanceChangesRepository;
            _indexerClient = indexerClient;
            _mainChainRepository = mainChainRepository;
            _log = log;
        }

        public async Task SaveAddressChangesAsync(int fromBlockHeight, string[] addresses)
        {
            var semaphore = new SemaphoreSlim(100);
            var tasksToAwait = new List<Task>();
            var mainChain = await _mainChainRepository.GetMainChainAsync();
            foreach (var address in addresses)
            {
                var balanceId = BalanceIdHelper.Parse(address, Network.Main);

                var changesTask = _indexerClient.GetConfirmedBalanceChangesAsync(balanceId, mainChain, semaphore, fromBlockHeight, mainChain.Height).ContinueWith(
                    async task =>
                    {
                        try
                        {

                            var coloredChanges = task.Result.SelectMany(p => p.GetColoredChanges(Network.Main)).ToList();

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
                                Address = address,
                                BlockHash = p.BlockHash
                            }).ToArray();


                            await _balanceChangesRepository.AddAsync(address, balanceChanges);
                        }
                        catch (Exception e)
                        {
                            await _log.WriteFatalError("BalanceChangesService", "SaveAddressChangesAsync", address, e);
                        }
                    });

                tasksToAwait.Add(changesTask.Unwrap());
            }

            await Task.WhenAll(tasksToAwait);
        }
    }
}
