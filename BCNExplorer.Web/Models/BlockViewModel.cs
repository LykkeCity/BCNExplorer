using System;
using System.Collections.Generic;
using Providers.TransportTypes;
using Providers.TransportTypes.Ninja;

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

        public static BlockViewModel Create(BlockDTO blockDto)
        {
            return new BlockViewModel
            {
                Confirmations = blockDto.Confirmations,
                Difficulty = blockDto.Difficulty,
                Hash = blockDto.Hash,
                Height = blockDto.Height,
                MerkleRoot = blockDto.MerkleRoot,
                Nonce = blockDto.Nonce,
                PreviousBlock = blockDto.PreviousBlock,
                Time = blockDto.Time,
                TransactionIdList = new TransactionIdList(blockDto.TransactionIds, PageSize),
                TotalTransactions = blockDto.TotalTransactions
            };
        }
    }
}