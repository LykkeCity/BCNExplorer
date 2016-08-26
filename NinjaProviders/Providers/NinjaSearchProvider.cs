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

            if (IsType<BlockContract>(responce))
            {
                return NinjaType.Block;
            }
            if (IsType<TransactionContract>(responce))
            {
                return NinjaType.Transaction;
            }

            return null;
        }

        private bool IsType<T>(string responce)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responce)!=null;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
