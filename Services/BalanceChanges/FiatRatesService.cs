using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.BalanceReport;
using Providers.Providers.Lykke.API;

namespace Services.BalanceChanges
{
    public class FiatRatesService
    {
        private readonly LykkeAPIProvider _lykkeApiProvider;

        public FiatRatesService(LykkeAPIProvider lykkeApiProvider)
        {
            _lykkeApiProvider = lykkeApiProvider;
        }

        public async Task<IEnumerable<IFiatRates>> GetRatesAsync(DateTime at, IEnumerable<string> quotingCurrencies, IEnumerable<string> btcAssetIds)
        {
            var assets = await _lykkeApiProvider.GetAssetsAsync();
            var pairs = await _lykkeApiProvider.GetAssetPairDictionary();

            var baseAssetIds = assets.Where(p => btcAssetIds.Contains(p.BitcoinAssetId) || p.Name == "BTC").Select(p => p.Id).ToList();

            var assetPairs = pairs.Where(p => (quotingCurrencies.Contains(p.QuotingAssetId) || quotingCurrencies.Contains(p.BaseAssetId)) && baseAssetIds.Contains(p.BaseAssetId));
            
            var rates = (await _lykkeApiProvider.GetAssetPairRatesAtTimeAsync(at, AssetPairRateHistoryPeriod.Day,
                assetPairs.Select(p => p.Id).ToArray())).ToDictionary(p => p.Id, p => p.Bid);

            var result = new List<FiatRate>();
            foreach (var currency in quotingCurrencies)
            {
                var relatedPairs = pairs.Where(p => p.QuotingAssetId == currency || p.BaseAssetId == currency);
                var priceDictionary = new Dictionary<string, IPrice>();
  
                foreach (var pair in relatedPairs)
                {
                    if (rates.ContainsKey(pair.Id))
                    {
                        var baseAsset = assets.FirstOrDefault(p => p.Id == pair.BaseAssetId);

                        if (baseAsset?.BitcoinAssetId != null)
                        {
                            priceDictionary[baseAsset.BitcoinAssetId] = Price.Create(rates[pair.Id], pair.Accuracy);
                        }

                        priceDictionary[pair.BaseAssetId] = Price.Create(rates[pair.Id], pair.Accuracy);

                        var invertedAsset = assets.FirstOrDefault(p => p.Id == pair.QuotingAssetId);

                        if (invertedAsset?.BitcoinAssetId != null)
                        {
                            priceDictionary[invertedAsset.BitcoinAssetId] = Price.Create(1/rates[pair.Id], pair.InvertedAccuracy);
                        }

                        priceDictionary[pair.QuotingAssetId] = Price.Create(Math.Round(1 / rates[pair.Id], pair.InvertedAccuracy), pair.InvertedAccuracy);
                    }

                    var sameCurrencyAsset = assets.FirstOrDefault(p => p.Name == currency);
                    if (sameCurrencyAsset?.BitcoinAssetId != null)
                    {
                        priceDictionary[sameCurrencyAsset.BitcoinAssetId] = Price.Create(1, sameCurrencyAsset.Accuracy);
                    }

                }

                result.Add(FiatRate.Create(currency, priceDictionary));
            }

            return result;
        } 
    }
}
