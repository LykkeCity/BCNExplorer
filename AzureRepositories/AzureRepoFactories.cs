using AzureRepositories.AssetCoinHolders;
using AzureRepositories.AssetDefinition;
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

        public static AssetDefinitionParseBlockCommandProducer CreateAssetDefinitionParseBlockCommandProducer(BaseSettings baseSettings, ILog log)
        {
            return new AssetDefinitionParseBlockCommandProducer(new AzureQueueExt(baseSettings.Db.AssetsConnString, JobsQueueNames.AssetDefinitionParseBlockCommands));
        }

        public static AssetChangesParseBlockCommandProducer CreateAssetChangesParseBlockCommandProducer(BaseSettings baseSettings, ILog log)
        {
            return new AssetChangesParseBlockCommandProducer(new AzureQueueExt(baseSettings.Db.AssetsConnString, JobsQueueNames.AssetChangesParseBlockCommands));
        }

        public static AssetDefinitionParsedBlockRepository CreateAssetParsedBlockRepository(BaseSettings baseSettings, ILog log)
        {
            return new AssetDefinitionParsedBlockRepository(new AzureTableStorage<AssetDefinitionParsedBlockEntity>(baseSettings.Db.AssetsConnString, "AssetParsedBlocks", log));
        }
    }
}
