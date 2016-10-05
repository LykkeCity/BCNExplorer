using System;
using System.Threading.Tasks;
using Common.Log;
using JobsCommon;
using Microsoft.Azure.WebJobs;
using Providers.BlockChainReader;

namespace AssetScanner.Functions
{
    public class AssetCreatorFunctions
    {
        private readonly ILog _log;

        public AssetCreatorFunctions(ILog log)
        {
            _log = log;
        }


    }
}
