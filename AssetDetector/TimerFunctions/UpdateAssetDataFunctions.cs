using System.Linq;
using System.Threading.Tasks;
using AzureRepositories.Asset;
using Common;
using Common.Log;
using Core.Asset;
using Microsoft.Azure.WebJobs;

namespace AssetScanner.TimerFunctions
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

        //public async Task UpdateAssets([TimerTrigger("00:30:00", RunOnStartup = true)] TimerInfo timer)
        //{
        //    var assetsToUpdate = await _assetDefinitionRepository.GetAllAsync();

        //    await _log.WriteInfo("AssetUpdaterFunctions", "UpdateAssets", assetsToUpdate.ToJson(), "Started");

        //    var updUrls = assetsToUpdate.Select(p => p.AssetDefinitionUrl).ToArray();

        //    await _assetDataCommandProducer.CreateUpdateAssetDataCommand(updUrls);

        //    await _log.WriteInfo("AssetUpdaterFunctions", "UpdateAssets", assetsToUpdate.ToJson(), "Done");
        //}

        public async Task UpdateEmptyAssets([TimerTrigger("00:30:00", RunOnStartup = true)] TimerInfo timer)
        {
            var assetsToUpdate = await _assetDefinitionRepository.GetAllEmptyAsync();
            
            await _log.WriteInfo("AssetUpdaterFunctions", "UpdateEmptyAssets", assetsToUpdate.ToJson(), "Started");

            var updUrls = assetsToUpdate.Select(p => p.AssetDefinitionUrl).ToArray();

            await _assetDataCommandProducer.CreateUpdateAssetDataCommand(updUrls);
            await _assetDefinitionRepository.RemoveEmptyAsync(updUrls);

            await _log.WriteInfo("AssetUpdaterFunctions", "UpdateEmptyAssets", assetsToUpdate.ToJson(), "Done");
        }
    }
}
