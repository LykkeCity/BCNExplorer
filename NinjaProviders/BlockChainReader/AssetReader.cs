using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Settings;
using Providers.Contracts.Asset;

namespace Providers.BlockChainReader
{
    public class AssetReader
    {
        private readonly HttpReader _httpReader;

        public AssetReader(HttpReader httpReader)
        {
            _httpReader = httpReader;
        }

        private async Task<IEnumerable<AssetContract>> GetAllAsync(IEnumerable<string> absUrls)
        {
            var assets = new ConcurrentStack<AssetContract>();
            
            var populateTasks = absUrls.Select(url => _httpReader.GetAsync<AssetContract>(url).ContinueWith(task =>
            {
                if (task.Result != null)
                {
                    assets.Push(task.Result);
                }
            }));

            await Task.WhenAll(populateTasks);

            return assets.ToList();
        }

        public async Task<Dictionary<string, AssetContract>> GetDictionaryAsync(IEnumerable<string> absUrls)
        {
            var assets = await GetAllAsync(absUrls);
            var result = new Dictionary<string, AssetContract>(StringComparer.OrdinalIgnoreCase);

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
