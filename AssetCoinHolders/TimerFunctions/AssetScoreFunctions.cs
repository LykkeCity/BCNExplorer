using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using Core.Asset;
using Microsoft.Azure.WebJobs;
using Providers.Helpers;

namespace AssetCoinHoldersScanner.TimerFunctions
{
    public class AssetScoreFunctions
    {
        private readonly IAssetCoinholdersIndexRepository _indexRepository;
        private readonly ILog _log;
        private readonly IAssetService _assetService;
        
        public AssetScoreFunctions(IAssetCoinholdersIndexRepository indexRepository, ILog log, IAssetService assetService)
        {
            _indexRepository = indexRepository;
            _log = log;
            _assetService = assetService;
        }

        public async Task UpdateAssetScores([TimerTrigger("01:00:00", RunOnStartup = true)] TimerInfo timer)
        {
            try
            {
                await _log.WriteInfo("AssetScoreFunctions", "UpdateAssetScores", null, "Started");
                var indexes = (await _indexRepository.GetAllAsync()).ToList();
                var counter = indexes.Count();
                foreach (var index in indexes)
                {
                    Console.WriteLine(counter);
                    counter--;
                    var score = AssetScoreHelper.CalculateAssetScore(await _assetService.GetAssetAsync(index.AssetIds.FirstOrDefault()), index, indexes);

                    await _indexRepository.SetScoreAsync(index, score);
                }
                await _log.WriteInfo("AssetScoreFunctions", "UpdateAssetScores", null, "Done");
            }
            catch (Exception e)
            {
                await _log.WriteError("AssetScoreFunctions", "UpdateAssetScores", null, e);
                throw;
            }
        }
    }
}
