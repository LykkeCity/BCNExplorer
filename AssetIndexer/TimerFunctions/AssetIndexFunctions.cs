using System.Linq;
using System.Threading.Tasks;
using AzureRepositories.AssetCoinHolders;
using Common.Log;
using Core.Asset;
using Microsoft.Azure.WebJobs;

namespace AssetIndexer.TimerFunctions
{
    public class AssetIndexFunctions
    {
        private readonly ILog _log;
        private readonly AssetCoinholdersIndexesCommandProducer _assetCoinholdersIndexesCommandProducer;
        private readonly IAssetDefinitionRepository _assetDefinitionRepository;

        public AssetIndexFunctions(ILog log, 
            AssetCoinholdersIndexesCommandProducer assetCoinholdersIndexesCommandProducer,
            IAssetDefinitionRepository assetDefinitionRepository)
        {
            _log = log;
            _assetCoinholdersIndexesCommandProducer = assetCoinholdersIndexesCommandProducer;
            _assetDefinitionRepository = assetDefinitionRepository;
        }

        public async Task UpdateIndexCoinholdersData([TimerTrigger("23:00:00", RunOnStartup = true)] TimerInfo timer)
        {
            await _log.WriteInfo("AssetIndexFunctions", "UpdateIndexCoinholdersData", null, "Started");

            var assets = await _assetDefinitionRepository.GetAllAsync();
            await _assetCoinholdersIndexesCommandProducer.CreateAssetCoinholdersUpdateIndexCommand(assets.ToArray());

            await _log.WriteInfo("AssetIndexFunctions", "UpdateIndexCoinholdersData", null, "Done");
        }
    }
}
