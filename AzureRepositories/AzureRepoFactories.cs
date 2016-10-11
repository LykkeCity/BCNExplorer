using AzureRepositories.Asset;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Log;
using Core.Settings;

namespace AzureRepositories
{
    public static class AzureRepoFactories
    {
        public static AssetDefinitionRepository CreateAssetDefinitionsRepository(BaseSettings baseSettings, ILog log)
        {
            return new AssetDefinitionRepository(new AzureTableStorage<AssetDefinitionEntity>(baseSettings.Db.AssetsConnString, "AssetDefinitions", log));
        }

        public static AssetDataCommandProducer CreateUpdateAssetDataCommandProducer(BaseSettings baseSettings, ILog log)
        {
            return new AssetDataCommandProducer(new AzureQueueExt(baseSettings.Db.AssetsConnString,  JobsQueueNames.UpdateAssetDataCommands));
        }

        public static ParseBlockCommandProducer CreateParseBlockCommandProducer(BaseSettings baseSettings, ILog log)
        {
            return new ParseBlockCommandProducer(new AzureQueueExt(baseSettings.Db.AssetsConnString, JobsQueueNames.ParseBlockCommands));
        }
        
        public static AssetParsedBlockRepository CreateAssetParsedBlockRepository(BaseSettings baseSettings, ILog log)
        {
            return new AssetParsedBlockRepository(new AzureTableStorage<AssetParsedBlockEntity>(baseSettings.Db.AssetsConnString, "AssetParsedBlocks", log));
        }
    }
}
