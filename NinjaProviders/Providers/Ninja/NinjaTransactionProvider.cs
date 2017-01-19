using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Providers.BlockChainReader;
using Providers.Contracts.Ninja;

namespace Providers.Providers.Ninja
{
    public class NinjaTransaction
    {
        public string TransactionId { get; set; }
        
        public string Hex { get; set; }
        
        public double Fees { get; set; }
        
        public BlockMinInfo Block { get; set; }
        
        public IEnumerable<InOut> Outputs { get; set; }
        
        public IEnumerable<InOut> Inputs { get; set; }
        
        public DateTime FirstSeen { get; set; }

        public static NinjaTransaction Create(TransactionContract contract)
        {
            return new NinjaTransaction
            {
                Fees = contract.Fees,
                Block = BlockMinInfo.Create(contract.Block),
                FirstSeen = contract.FirstSeen,
                Hex = contract.Hex,
                Inputs = contract.Inputs.Select(InOut.Create),
                Outputs = contract.Outputs.Select(InOut.Create),
                TransactionId = contract.TransactionId
            };
        }

        #region Classes
        public class BlockMinInfo
        {
            public double Confirmations { get; set; }
            
            public double Height { get; set; }
            
            public string BlockId { get; set; }
            
            public DateTime BlockTime { get; set; }

            public static BlockMinInfo Create(TransactionContract.BlockContract contract)
            {
                if (contract != null)
                {
                    return new BlockMinInfo
                    {
                        BlockId = contract.BlockId,
                        BlockTime = contract.BlockTime,
                        Confirmations = contract.Confirmations,
                        Height = contract.Height
                    };
                }

                return null;
            }
        }

        public class InOut
        {
            public string Address { get; set; }
            
            public string TransactionId { get; set; }
            
            public int Index { get; set; }
            
            public double Value { get; set; }
            
            public string ScriptPubKey { get; set; }
            
            public string AssetId { get; set; }
            
            public double Quantity { get; set; }

            public static InOut Create(InOutContract contract)
            {
                return new InOut
                {
                    Address = contract.Address,
                    AssetId = contract.AssetId,
                    Index = contract.Index,
                    Quantity = contract.Quantity,
                    ScriptPubKey = contract.ScriptPubKey,
                    TransactionId = contract.TransactionId,
                    Value = contract.Value
                };
            }
        }

        #endregion
    }

    public class NinjaTransactionProvider
    {
        private readonly NinjaBlockChainReader _blockChainReader;

        public NinjaTransactionProvider(NinjaBlockChainReader blockChainReader)
        {
            _blockChainReader = blockChainReader;
        }

        public async Task<NinjaTransaction> GetAsync(string id)
        {
            var responce = await _blockChainReader.GetAsync<TransactionContract>($"transactions/{id}?colored=true");
            if (responce == null)
            {
                return null;
            }

            return NinjaTransaction.Create(responce);
            
        }
    }
}
