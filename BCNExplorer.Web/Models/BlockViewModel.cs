using System;
using Core.Block;

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
        public TransactionIdList TransactionIdList { get; set; }
        private const int PageSize = 20;

        public static BlockViewModel Create(IBlock ninjaBlock)
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
                TransactionIdList = new TransactionIdList(ninjaBlock.TransactionIds, PageSize),
                TotalTransactions = ninjaBlock.TotalTransactions
            };
        }
    }
}