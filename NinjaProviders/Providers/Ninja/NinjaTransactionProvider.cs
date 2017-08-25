using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Settings;
using NBitcoin;
using NBitcoin.DataEncoders;
using Providers.BlockChainReader;
using Providers.Contracts.Ninja;
using Providers.Helpers;

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

        public static NinjaTransaction Create(TransactionContract contract, Network network)
        {
            return new NinjaTransaction
            {
                Fees = contract.Fees,
                Block = BlockMinInfo.Create(contract.Block),
                FirstSeen = contract.FirstSeen,
                Hex = contract.Hex,
                Inputs = contract.Inputs.Select(p => InOut.Create(p, network)),
                Outputs = contract.Outputs.Select(p => InOut.Create(p, network)),
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

            public static InOut Create(InOutContract contract, Network network)
            {
                string address = null;
                try
                {
                    address = GetScriptFromBytes(contract.ScriptPubKey).GetDestinationAddress(network).ToWif();
                }
                catch (Exception e)
                {
                }
                return new InOut
                {
                    Address = address,
                    AssetId = contract.AssetId,
                    Index = contract.Index,
                    Quantity = contract.Quantity ?? 0,
                    ScriptPubKey = contract.ScriptPubKey,
                    TransactionId = contract.TransactionId,
                    Value = contract.Value
                };
            }
            //todo move to helper class
            private static Script GetScriptFromBytes(string data)
            {
                var bytes = Encoders.Hex.DecodeData(data);
                var script = Script.FromBytesUnsafe(bytes);
                bool hasOps = false;
                var reader = script.CreateReader();
                foreach (var op in reader.ToEnumerable())
                {
                    hasOps = true;
                    if (op.IsInvalid || (op.Name == "OP_UNKNOWN" && op.PushData == null))
                        return null;
                }
                return !hasOps ? null : script;
            }
        }

        #endregion
    }

    public class NinjaTransactionProvider
    {
        private readonly NinjaBlockChainReader _blockChainReader;
        private readonly BaseSettings _baseSettings;

        public NinjaTransactionProvider(NinjaBlockChainReader blockChainReader, BaseSettings baseSettings)
        {
            _blockChainReader = blockChainReader;
            _baseSettings = baseSettings;
        }

        public async Task<NinjaTransaction> GetAsync(string id)
        {
            var responce = await _blockChainReader.GetAsync<TransactionContract>($"transactions/{id}?colored=true");
            if (responce == null)
            {
                return null;
            }

            return NinjaTransaction.Create(responce, _baseSettings.UsedNetwork());
            
        }
    }
}
