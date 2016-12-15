using System;
using System.Linq;
using System.Threading.Tasks;
using AzureRepositories.AssetDefinition;
using Common.Log;
using Core.Asset;
using Microsoft.Azure.WebJobs;

namespace AssetDefinitionScanner.TimerFunctions
{
    public class UpdateAssetDataFunctions
    {
        private readonly IAssetDefinitionRepository _assetDefinitionRepository;
        private readonly ILog _log;
        private readonly AssetDataCommandProducer _assetDataCommandProducer;

        public UpdateAssetDataFunctions(IAssetDefinitionRepository assetDefinitionRepository, ILog log,  AssetDataCommandProducer assetDataCommandProducer)
        {
            _assetDefinitionRepository = assetDefinitionRepository;
            _log = log;
            _assetDataCommandProducer = assetDataCommandProducer;
        }

        public async Task UpdateAssets([TimerTrigger("23:00:00", RunOnStartup = true)] TimerInfo timer)
        {
            try
            {
                await _log.WriteInfo("UpdateAssetDataFunctions", "UpdateAssets", null, "Started");

                var assetsToUpdate = await _assetDefinitionRepository.GetAllAsync();

                var updUrls = assetsToUpdate.Select(p => p.AssetDefinitionUrl).ToArray();

                await _assetDataCommandProducer.CreateUpdateAssetDataCommand(updUrls);

                await _log.WriteInfo("UpdateAssetDataFunctions", "UpdateAssets", updUrls.Length.ToString(), "Done");
            }
            catch (Exception e)
            {
                await _log.WriteError("UpdateAssetDataFunctions", "UpdateAssets", null, e);
            }
        }

        //public async Task UpdateEmptyAssets([TimerTrigger("01:00:00", RunOnStartup = true)] TimerInfo timer)
        //{
        //    var assetsToUpdate = await _assetDefinitionRepository.GetAllEmptyAsync();

        //    await _log.WriteInfo("AssetUpdaterFunctions", "UpdateEmptyAssets", assetsToUpdate.ToJson(), "Started");

        //    var updUrls = assetsToUpdate.Select(p => p.AssetDefinitionUrl).ToArray();

        //    await _assetDataCommandProducer.CreateUpdateAssetDataCommand(updUrls);
        //    await _assetDefinitionRepository.RemoveEmptyAsync(updUrls);

        //    await _log.WriteInfo("AssetUpdaterFunctions", "UpdateEmptyAssets", assetsToUpdate.ToJson(), "Done");
        //}
    }
}
