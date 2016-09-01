using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Settings;
using Providers.Contracts.Lykke;

namespace Providers.BlockChainReader
{
    public class LykkeBlockChainReader
    {
        private readonly HttpReader _httpReader;

        public LykkeBlockChainReader(HttpReader httpReader)
        {
            _httpReader = httpReader;
        }

        private async Task<IEnumerable<LykkeAssetContract>> GetAllAsync(IEnumerable<string> absUrls)
        {
            var assets = new ConcurrentStack<LykkeAssetContract>();
            
            var populateTasks = absUrls.Select(url => _httpReader.GetAsync<LykkeAssetContract>(url).ContinueWith(task =>
            {
                if (task.Result != null)
                {
                    assets.Push(task.Result);
                }
            }));

            await Task.WhenAll(populateTasks);

            return assets.ToList();
        }

        public async Task<Dictionary<string, LykkeAssetContract>> GetDictionaryAsync(IEnumerable<string> absUrls)
        {
            var assets = await GetAllAsync(absUrls);
            var result = new Dictionary<string, LykkeAssetContract>(StringComparer.OrdinalIgnoreCase);

            foreach (var asset in assets)
            {
                result[asset.Name] = asset;
                result[asset.NameShort] = asset;

                foreach (var assetId in asset.AssetIds ?? Enumerable.Empty<string>())
                {
                    result[assetId] = asset;
                }
            }

            return result;
        }
    }
}
