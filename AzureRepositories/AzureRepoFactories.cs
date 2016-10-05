//using AzureRepositories.Grab;
//using AzureStorage.Tables;
//using Common.Log;
//using Core.Settings;

using AzureRepositories.Asset;
using AzureStorage.Tables;
using Common.Log;
using Core.Settings;

namespace AzureRepositories
{
    public static class AzureRepoFactories
    {
        public static AssetRepository CreateAssetRepository(BaseSettings baseSettings, ILog log)
        {
            return new AssetRepository(new AzureTableStorage<AssetEntity>(baseSettings.Db.AssetsConnString, "Assets", log));
        }
    }
}
