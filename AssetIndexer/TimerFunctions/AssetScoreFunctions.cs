using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Core.Asset;
using Microsoft.Azure.WebJobs;
using Providers.Helpers;

namespace AssetIndexer.TimerFunctions
{
    public class AssetScoreFunctions
    {
        private readonly IAssetCoinholdersIndexRepository _indexRepository;
        private readonly IAssetScoreRepository _assetScoreRepository;
        private readonly ILog _log;
        private readonly IAssetService _assetService;
        
        public AssetScoreFunctions(IAssetCoinholdersIndexRepository indexRepository, 
            ILog log, 
            IAssetService assetService, 
            IAssetScoreRepository assetScoreRepository)
        {
            _indexRepository = indexRepository;
            _log = log;
            _assetService = assetService;
            _assetScoreRepository = assetScoreRepository;
        }

        public async Task UpdateAssetScores([TimerTrigger("23:59:00", RunOnStartup = false)] TimerInfo timer)
        {
            try
            {
                await _log.WriteInfo("AssetScoreFunctions", "UpdateAssetScores", null, "Started");
                var indexes = (await _indexRepository.GetAllAsync()).ToList();
                foreach (var index in indexes)
                {
                    var score = AssetScoreHelper.CalculateAssetScore(await _assetService.GetAssetAsync(index.AssetIds.FirstOrDefault()), index, indexes);

                    await _log.WriteInfo("AssetScoreFunctions", "UpdateAssetScores", index.AssetIds.FirstOrDefault(), "Done");
                    await _assetScoreRepository.InsertOrReplaceAsync(AssetScore.Create(index.AssetIds, score));
                }
                await _log.WriteInfo("AssetScoreFunctions", "UpdateAssetScores", "All", "Done");
            }
            catch (Exception e)
            {
                await _log.WriteError("AssetScoreFunctions", "UpdateAssetScores", null, e);
                throw;
            }
        }
    }
}
