using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinjaProviders.Contracts;

namespace NinjaProviders.TransportTypes
{
    public class NinjaTransaction
    {
        public string TransactionId { get; set; }
        public IList<InOut> TransactionIn { get; set; }
        public IList<InOut> TransactionsOut { get; set; }
        public bool IsCoinBase { get; set; }
        public bool IsColor { get; set; }
        public string Hex { get; set; }
        public double Fees { get; set; }
        public BlockMinInfo Block { get; set; }
        
        #region Classes 

        public class InOut
        {
            public string TransactionId { get; set; }
            public string Address { get; set; }
            public int Index { get; set; }
            public double Value { get; set; }
            public string AssetId { get; set; }
            public double Quantity { get; set; }

            public static IEnumerable<InOut> Create(IEnumerable<TransactionContract.BitCoinInOutContract> inOut)
            {
                return inOut.Select(x => new InOut()
                {
                    TransactionId = x.TransactionId,
                    Address = x.Address,
                    Index = x.Index,
                    Value = x.Value,
                    AssetId = x.AssetId,
                    Quantity = x.Quantity
                });
            } 
        }

        public class BlockMinInfo
        {
            public string BlockId { get; set; }
            public double Height { get; set; }
            public DateTime Time { get; set; }
            public double Confirmations { get; set; }

            public static BlockMinInfo Create(TransactionContract.BlockContract blockContract)
            {
                return new BlockMinInfo
                {
                    BlockId = blockContract.BlockId,
                    Confirmations = blockContract.Confirmations,
                    Height = blockContract.Height,
                    Time = blockContract.BlockTime
                };
            }
        }

        #endregion
    }
}
