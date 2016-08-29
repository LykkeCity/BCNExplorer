using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinjaProviders.BlockChainReader;
using NinjaProviders.Contracts;
using NinjaProviders.TransportTypes;

namespace NinjaProviders.Providers
{
    public class NinjaSearchProvider
    {
        private readonly NinjaBlockChainReader _blockChainReader;

        public NinjaSearchProvider(NinjaBlockChainReader blockChainReader)
        {
            _blockChainReader = blockChainReader;
        }

        public async Task<NinjaType?> GetTypeAsync(string id)
        {
            var responce = await _blockChainReader.DoRequest($"whatisit/{id}");

            if (IsBlock(responce))
            {
                return NinjaType.Block;
            }
            if (IsTransaction(responce))
            {
                return NinjaType.Transaction;
            }

            return null;
        }

        private bool IsBlock(string responce)
        {
            var deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<BlockHeaderContract>(responce);
            return deserialized?.AdditionalInformation?.BlockId != null;
        }

        private bool IsTransaction(string responce)
        {
            var deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<TransactionContract>(responce);
            return deserialized.TransactionId != null;
        }
    }
}
