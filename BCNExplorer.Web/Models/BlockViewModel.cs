using System;
using System.Collections.Generic;
using System.Linq;
using Core.Asset;
using Core.Block;
using Core.Channel;

namespace BCNExplorer.Web.Models
{
    public class BlockViewModel
    {
        public string Hash { get; set; }
        public long Height { get; set; }
        public DateTime Time { get; set; }
        public long Confirmations { get; set; }
        public double Difficulty { get; set; }
        public string MerkleRoot { get; set; }
        public long Nonce { get; set; }
        public int TotalTransactions { get; set; }
        public string PreviousBlock { get; set; }
        public bool ShowPreviousBlock => !string.IsNullOrEmpty(Hash) && Height != 0;
        public long PreviousBlockHeight => Height-1;
        public long NextBlockHeight { get; set; }
        public bool ShowNextBlock { get; set; }

        public TransactionIdList AllTransactionIdList { get; set; }
        public TransactionIdList ColoredTransactionIdList { get; set; }
        public TransactionIdList UncoloredTransactionIdList { get; set; }


        public OffchainChannelPagedList OffchainChannelPagedList { get; set; }

        public static BlockViewModel Create(IBlock ninjaBlock, 
            IBlockHeader lastBlock,
            long offchainChannelsCount, 
            int offchainTransactionsPageSize)
        {
            return new BlockViewModel
            {
                Confirmations = ninjaBlock.Confirmations,
                Difficulty = ninjaBlock.Difficulty,
                Hash = ninjaBlock.Hash,
                Height = ninjaBlock.Height,
                MerkleRoot = ninjaBlock.MerkleRoot,
                Nonce = ninjaBlock.Nonce,
                PreviousBlock = ninjaBlock.PreviousBlock,
                Time = ninjaBlock.Time,
                AllTransactionIdList = new TransactionIdList(ninjaBlock.AllTransactionIds),
                ColoredTransactionIdList = new TransactionIdList(ninjaBlock.ColoredTransactionIds),
                UncoloredTransactionIdList = new TransactionIdList(ninjaBlock.UncoloredTransactionIds),
                TotalTransactions = ninjaBlock.TotalTransactions,
                ShowNextBlock = ninjaBlock.Height < lastBlock?.Height,
                NextBlockHeight = ninjaBlock.Height + 1,
                OffchainChannelPagedList = OffchainChannelPagedList.Create(
                    offchainChannelsCount, 
                    offchainTransactionsPageSize,
                    (url, page) => url.Action("OffchainChannelPage", "Block", new { block = ninjaBlock.Hash, page = page })
                )
            };
        }
    }
}