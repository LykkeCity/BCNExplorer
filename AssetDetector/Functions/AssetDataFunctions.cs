using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureRepositories;
using Common.Log;
using Core.Asset;
using JobsCommon;
using Microsoft.Azure.WebJobs;
using Providers.BlockChainReader;
using Providers.TransportTypes.Asset;

namespace AssetScanner.Functions
{
    public class AssetDataFunctions
    {
        private readonly AssetReader _assetReader;
        private readonly IAssetRepository _assetRepository;
        private readonly ILog _log;

        public AssetDataFunctions(IAssetRepository assetRepository, ILog log, AssetReader assetReader)
        {
            _assetRepository = assetRepository;
            _log = log;
            _assetReader = assetReader;
        }

        //public async Task UpdateAssets([TimerTrigger("00:20:00", RunOnStartup = true)] TimerInfo timer)
        //{
        //    await _log.WriteInfo("AssetUpdaterFunctions", "UpdateAssets", null, "Update assets started");

        //    var assetsToUpdate =  await _assetRepository.GetAllAsync();
        //    var updatedAssets = await _assetReader.ReadAssetDataAsync(assetsToUpdate.Select(p => p.AssetDefinitionUrl).ToArray());

        //    await _assetRepository.InsertOrReplaceAsync(updatedAssets.Select(AssetDefinition.Create).ToArray());
        //}

        //public async Task CreateAssetData([QueueTrigger(JobsQueueNames.AddNewAssetsQueueName)] string message, DateTimeOffset insertionTime)
        //{
        //    await _log.WriteInfo("AssetUpdaterFunctions", "UpdateAssets", "CreateAssetData", $" {message} started {DateTime.Now} ");

        //    var assetData = await _assetReader.ReadAssetDataAsync(message);
        //    await _assetRepository.InsertOrReplaceAsync(AssetDefinition.Create(assetData));
        //}

        
    }
}
