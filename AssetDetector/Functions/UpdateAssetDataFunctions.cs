using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureRepositories;
using AzureRepositories.Asset;
using Common;
using Common.Log;
using Core.Asset;
using JobsCommon;
using Microsoft.Azure.WebJobs;
using Providers.BlockChainReader;
using Providers.TransportTypes.Asset;

namespace AssetScanner.Functions
{
    public class UpdateAssetDataFunctions
    {
        private readonly IAssetRepository _assetRepository;
        private readonly ILog _log;
        private readonly AssetDataCommandProducer _assetDataCommandProducer;

        public UpdateAssetDataFunctions(IAssetRepository assetRepository, ILog log,  AssetDataCommandProducer assetDataCommandProducer)
        {
            _assetRepository = assetRepository;
            _log = log;
            _assetDataCommandProducer = assetDataCommandProducer;
        }

        public async Task UpdateAssets([TimerTrigger("00:30:00", RunOnStartup = true)] TimerInfo timer)
        {
            var assetsToUpdate = await _assetRepository.GetAllAsync();

            await _log.WriteInfo("AssetUpdaterFunctions", "UpdateAssets", assetsToUpdate.ToJson(), "Update assets started");

            await _assetDataCommandProducer.CreateUpdateAssetDataCommand(
                    assetsToUpdate.Select(p => p.AssetDefinitionUrl).ToArray());
        }
    }
}
