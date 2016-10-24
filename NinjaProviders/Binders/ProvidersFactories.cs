using Common.Log;
using Core.Settings;
using Microsoft.WindowsAzure.Storage.Auth;
using NBitcoin.Indexer;

namespace Providers.Binders
{
    public static class ProvidersFactories
    {
        public static IndexerClient CreateNinjaIndexerClient(BaseSettings baseSettings, ILog log)
        {
            var indexerConfiguration = new IndexerConfiguration
            {
                StorageCredentials = new StorageCredentials(baseSettings.NinjaIndexerCredentials.AzureName, baseSettings.NinjaIndexerCredentials.AzureKey)
            };

            var result = new IndexerClient(indexerConfiguration) {ColoredBalance = true};

            return result;
        }
    }
}
