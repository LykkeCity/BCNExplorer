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
    }

    public class Asset:IAsset
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string BitcoinAssetId { get; set; }

        public static Asset Create(AssetContract source)
        {
            return new Asset
            {
                Id = source.id,
                Name = source.name,
                BitcoinAssetId = source.bitcoinAssetId
            };
        }
    }

    public interface IAssetPair
    {
        string Id { get;  }

        string Name { get;  }

        string BaseAssetId { get;  }

        string QuotingAssetId { get;  }
    }

    public class AssetPair : IAssetPair
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string BaseAssetId { get; set; }
        public string QuotingAssetId { get; set; }

        public static AssetPair Create(AssetPairContract source)
        {
            return new AssetPair
            {
                BaseAssetId = source.baseAssetId,
                Id = source.id,
                Name = source.name,
                QuotingAssetId = source.quotingAssetId
            };
        }
    }

    public interface IAssetPairRate
    {
        string Id { get; }
        decimal Rate { get; }
    }

    public class AssetPairRate : IAssetPairRate
    {
        public string Id { get; set; }
        public decimal Rate { get; set; }

        public static AssetPairRate Create(AssetPairRateContract source)
        {
            return new AssetPairRate
            {
                Id = source.id,
                Rate = source.ask
            };
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

        public async Task<IEnumerable<IAssetPairRate>> GetAssetPairRatesAtTimeAsync(DateTime time, params string[] pairs)
        {
            IEnumerable<AssetPairRateContract> resp;
            
            //api allows max 10 pairs at one request

            const int maxPairSize = 10;
            
            if (pairs.Length < maxPairSize)
            {
                resp = await GetAssetPairRatesAtTimeInnerAsync(time, pairs);
            }
            else
            {
               var tasks = new List<Task>();

                var concurrentBag = new ConcurrentBag<AssetPairRateContract>();

                foreach (var batch in pairs.Batch(maxPairSize))
                {
                    var tsk =
                        GetAssetPairRatesAtTimeInnerAsync(time, batch.ToArray()).ContinueWith(p =>
                        {
                            foreach (var assetPairRateContract in p.Result)
                            {
                                concurrentBag.Add(assetPairRateContract);
                            }
                        });

                    tasks.Add(tsk);
                }
                
                await Task.WhenAll(tasks);
                resp = concurrentBag.ToArray();
            }
            
            return resp.Select(AssetPairRate.Create);
        }

        private async Task<IEnumerable<AssetPairRateContract>> GetAssetPairRatesAtTimeInnerAsync(DateTime time,
            params string[] pairs)
        {
            return await _baseSettings.LykkeAPIUrl.AppendPathSegment("api/AssetPairs/rate/history")
                .PostJsonAsync(new {period = "Sec", DateTime = time, assetPairIds = pairs})
                .ReceiveJson<AssetPairRateContract[]>();
        }
    }
}
