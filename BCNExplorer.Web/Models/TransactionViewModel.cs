using System;
using System.Collections.Generic;
using System.Linq;
using NinjaProviders.TransportTypes;

namespace BCNExplorer.Web.Models
{
    public class TransactionViewModel
    {
        public string TransactionId { get; set; }
        public double Confirmations { get; set; }
        public DateTime Time { get; set; }
        public IEnumerable<InOut> TransactionIn { get; set; }
        public IEnumerable<InOut> TransactionsOut { get; set; }
        public bool IsColor { get; set; }
        public bool IsCoinBase { get; set; }

        public double Fees { get; set; }
        public int AssetsCount { get; set; }

        public BlockViewModel Block { get; set; }

        #region Classes 

        public class BlockViewModel
        {
            public double Confirmations { get; set; }

            public DateTime Time { get; set; }
     
            public double Height { get; set; }
            public string BlockId { get; set; }

            public static BlockViewModel Create(NinjaTransaction.BlockMinInfo blockMinInfo)
            {
                return new BlockViewModel
                {
                    Confirmations = blockMinInfo.Confirmations,
                    Height = blockMinInfo.Height,
                    Time = blockMinInfo.Time,
                    BlockId = blockMinInfo.BlockId
                };
            }
        }

        public class InOut
        {
            public string TransactionId { get; set; }
            public string Address { get; set; }
            public int Index { get; set; }
            public double Value { get; set; }
            public string AssetId { get; set; }
            public double Quantity { get; set; }

            public static IEnumerable<InOut> Create(IEnumerable<NinjaTransaction.InOut> source)
            {
                return source.Select(p => new InOut
                {
                    Address = p.Address,
                    AssetId = p.AssetId,
                    Index = p.Index,
                    Quantity = p.Quantity,
                    TransactionId = p.TransactionId,
                    Value = p.Value
                });
            } 
        }

        #endregion

        public static TransactionViewModel Create(NinjaTransaction ninjaTransaction)
        {
            var result = new TransactionViewModel
            {
                TransactionId = ninjaTransaction.TransactionId,
                Time = ninjaTransaction.Block.Time,
                IsCoinBase = ninjaTransaction.IsCoinBase,
                IsColor = ninjaTransaction.IsColor,
                Block = BlockViewModel.Create(ninjaTransaction.Block),
                Confirmations = ninjaTransaction.Block.Confirmations,
                Fees = ninjaTransaction.Fees,
                TransactionsOut = InOut.Create(ninjaTransaction.TransactionsOut),
                TransactionIn = InOut.Create(ninjaTransaction.TransactionIn)
            };


            return result;
        }
    }



}