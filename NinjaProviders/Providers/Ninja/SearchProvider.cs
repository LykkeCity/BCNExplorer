using System;
using System.Threading.Tasks;
using Providers.BlockChainReader;
using Providers.Contracts.Ninja;
using Providers.TransportTypes.Ninja;

namespace Providers.Providers.Ninja
{
    public class SearchProvider
    {
        private readonly NinjaBlockChainReader _blockChainReader;

        public SearchProvider(NinjaBlockChainReader blockChainReader)
        {
            _blockChainReader = blockChainReader;
        }

        public async Task<NinjaType?> GetTypeAsync(string id)
        {
            var responce = await _blockChainReader.GetAsync($"whatisit/{id}");

            if (IsBlock(responce))
            {
                return NinjaType.Block;
            }
            if (IsTransaction(responce))
            {
                return NinjaType.Transaction;
            }
            if (IsAddress(responce))
            {
                return NinjaType.Address;
            }

            return null;
        }

        private bool IsBlock(string responce)
        {
            var deserialized = TryDeserialize<BlockHeaderContract>(responce);
            return deserialized?.AdditionalInformation?.BlockId != null;
        }

        private bool IsTransaction(string responce)
        {
            var deserialized = TryDeserialize<TransactionContract>(responce);
            return deserialized?.TransactionId != null;
        }

        private bool IsAddress(string responce)
        {
            var deserialized = TryDeserialize<WhatIsItContract>(responce);
            return deserialized?.Type == WhatIsItContract.ColoredAddressType ||
                   deserialized?.Type == WhatIsItContract.UncoloredAddressType || 
                   deserialized?.Type == WhatIsItContract.ScriptAddressType;
        }

        private T TryDeserialize<T>(string source)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(source);
            }
            catch (Exception)
            {
                return default(T);
            }
        } 
    }
}
