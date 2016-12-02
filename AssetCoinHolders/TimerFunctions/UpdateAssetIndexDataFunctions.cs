using System.Linq;
using System.Threading.Tasks;
using AzureRepositories.AssetCoinHolders;
using Common.Log;
using Core.Asset;
using Microsoft.Azure.WebJobs;

namespace AssetCoinHoldersScanner.TimerFunctions
{
    public class UpdateAssetIndexDataFunctions
    {
        private readonly ILog _log;
        private readonly AssetCoinholdersIndexesCommandProducer _assetCoinholdersIndexesCommandProducer;
        private readonly IAssetDefinitionRepository _assetDefinitionRepository;

        public UpdateAssetIndexDataFunctions(ILog log, 
            AssetCoinholdersIndexesCommandProducer assetCoinholdersIndexesCommandProducer,
            IAssetDefinitionRepository assetDefinitionRepository)
        {
            _log = log;
            _assetCoinholdersIndexesCommandProducer = assetCoinholdersIndexesCommandProducer;
            _assetDefinitionRepository = assetDefinitionRepository;
        }

        public async Task UpdateIndexCoinholdersData([TimerTrigger("00:03:00", RunOnStartup = true)] TimerInfo timer)
        {
            await _log.WriteInfo("UpdateAssetIndexDataFunctions", "UpdateIndexCoinholdersData", null, "Started");

            var assets = await _assetDefinitionRepository.GetAllAsync();
            await _assetCoinholdersIndexesCommandProducer.CreateAssetCoinholdersUpdateIndexCommand(assets.ToArray());

            await _log.WriteInfo("UpdateAssetIndexDataFunctions", "UpdateIndexCoinholdersData", null, "Done");
        }
    }
}
