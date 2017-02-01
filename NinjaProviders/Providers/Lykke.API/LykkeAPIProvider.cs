using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Common;
using Core.Settings;
using Flurl;
using Flurl.Http;
using Microsoft.Data.Edm.Validation;
using Providers.Contracts.Lykke.API;

namespace Providers.Providers.Lykke.API
{
    public interface IAsset
    {
        string Id { get;  }
        string Name { get; }
        string BitcoinAssetId { get; }
        int Accuracy { get; }
    }

    public class Asset:IAsset
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string BitcoinAssetId { get; set; }
        public int Accuracy { get; set; }

        public static Asset Create(AssetContract source)
        {
            return new Asset
            {
                Id = source.id,
                Name = source.name,
                BitcoinAssetId = source.bitcoinAssetId,
                Accuracy = source.accuracy
            };
        }
    }

    public interface IAssetPair
    {
        string Id { get;  }

        string Name { get;  }

        string BaseAssetId { get;  }

        string QuotingAssetId { get;  }

        int Accuracy { get; }
        int InvertedAccuracy { get; }
    }

    public class AssetPair : IAssetPair
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string BaseAssetId { get; set; }
        public string QuotingAssetId { get; set; }
        public int Accuracy { get; set; }
        public int InvertedAccuracy { get; set; }

        public static AssetPair Create(AssetPairContract source)
        {
            return new AssetPair
            {
                BaseAssetId = source.baseAssetId,
                Id = source.id,
                Name = source.name,
                QuotingAssetId = source.quotingAssetId,
                Accuracy = source.accuracy,
                InvertedAccuracy = source.invertedAccuracy
            };
        }
    }

    public interface IAssetPairRate
    {
        string Id { get; }
        decimal Bid { get; }
        decimal Ask { get; }
    }

    public class AssetPairRate : IAssetPairRate
    {
        public string Id { get; set; }
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }

        public static AssetPairRate Create(AssetPairRateContract source)
        {
            if (source?.bid !=null && source?.ask != null)
            {
                return new AssetPairRate
                {
                    Id = source.id,
                    Bid = source.bid.Value,
                    Ask = source.ask.Value
                };
            }
            return null;
        }
    }

    public class LykkeAPIProvider
    {
        private readonly BaseSettings _baseSettings;

        public LykkeAPIProvider(BaseSettings baseSettings)
        {
            _baseSettings = baseSettings;
        }

        public async Task<IEnumerable<IAsset>> GetAssetsAsync()
        {
            var resp =
                await
                    _baseSettings.LykkeAPIUrl.AppendPathSegment("api/Assets/dictionary")
                        .GetAsync()
                        .ReceiveJson<AssetContract[]>();


            return resp.Select(Asset.Create);
        }

        public async Task<IEnumerable<IAssetPair>> GetAssetPairDictionary()
        {
            var resp = await
               _baseSettings.LykkeAPIUrl.AppendPathSegment("api/AssetPairs/dictionary")
                   .GetAsync()
                   .ReceiveJson<AssetPairContract[]>();
            return resp.Select(AssetPair.Create);
        }

        public async Task<IEnumerable<IAssetPairRate>> GetAssetPairRatesAtTimeAsync(DateTime time, AssetPairRateHistoryPeriod period, params string[] pairs)
        {
            var resp = new List<AssetPairRateContract>();

            //api allows max 10 pairs at one request

            const int maxPairSize = 10;
            
            if (pairs.Length < maxPairSize)
            {
                resp = (await GetAssetPairRatesAtTimeInnerAsync(time, period, pairs)).ToList();
            }
            else
            {
                foreach (var batch in pairs.Batch(maxPairSize))
                {
                    resp.AddRange(await GetAssetPairRatesAtTimeInnerAsync(time, period, batch.ToArray()));
                }
            }
            
            return resp.Select(AssetPairRate.Create).Where(p => p != null);
        }

        private async Task<IEnumerable<AssetPairRateContract>> GetAssetPairRatesAtTimeInnerAsync(DateTime time, AssetPairRateHistoryPeriod period,
            params string[] pairs)
        {
            try
            {
                return await _baseSettings.LykkeAPIUrl.AppendPathSegment("api/AssetPairs/rate/history")
                    .PostJsonAsync(new { period = period.ToString(), DateTime = time, assetPairIds = pairs })
                    .ReceiveJson<AssetPairRateContract[]>();
            }
            catch (Exception)
            {
                return Enumerable.Empty<AssetPairRateContract>();
            }
        }
    }

    public enum AssetPairRateHistoryPeriod
    {
        Sec,
        Minute,
        Hour,
        Day,
        Month
    }
}
