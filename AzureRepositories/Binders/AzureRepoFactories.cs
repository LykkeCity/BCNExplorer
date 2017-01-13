using AzureRepositories.Asset;
using AzureRepositories.AssetCoinHolders;
using AzureRepositories.AssetDefinition;
using AzureStorage.Blob;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Log;
using Core.Settings;
using MongoDB.Driver;

namespace AzureRepositories.Binders
{
    public static class AzureRepoFactories
    {
        public static AssetDefinitionRepository CreateAssetDefinitionsRepository(BaseSettings baseSettings, ILog log)
        {
            return new AssetDefinitionRepository(new AzureTableStorage<AssetDefinitionDefinitionEntity>(baseSettings.Db.AssetsConnString, "AssetDefinitions", log));
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

        public static AssetCoinholdersIndexesCommandProducer CreateAssetCoinholdersIndexesCommandProducer(BaseSettings baseSettings, ILog log)
        {
            return new AssetCoinholdersIndexesCommandProducer(new AzureQueueExt(baseSettings.Db.AssetsConnString, JobsQueueNames.AssetCoinhodersIndexesCommands));
        }

        public static AssetDefinitionParsedBlockRepository CreateAssetParsedBlockRepository(BaseSettings baseSettings, ILog log)
        {
            return new AssetDefinitionParsedBlockRepository(new AzureTableStorage<AssetDefinitionParsedBlockEntity>(baseSettings.Db.AssetsConnString, "AssetParsedBlocks", log));
        }

        public static AssetCoinholdersIndexRepository CreateAssetCoinholdersIndexRepository(BaseSettings baseSettings, ILog log)
        {
            return new AssetCoinholdersIndexRepository(new AzureTableStorage<AssetCoinholdersIndexEntity>(baseSettings.Db.AssetsConnString, "AssetCoinholdersIndexes", log));
        }

        public static AssetScoreRepository CreateAssetScoreRepository(BaseSettings baseSettings, ILog log)
        {
            return new AssetScoreRepository(new AzureTableStorage<AssetScoreEntity>(baseSettings.Db.AssetsConnString, "AssetScores", log));
        }

        public static AssetBalanceChangesRepository CreateAssetBalanceChangesRepository(BaseSettings baseSettings, ILog log)
        {
            var client = new MongoClient(baseSettings.Db.AssetBalanceChanges.ConnectionString);
            return new AssetBalanceChangesRepository(client.GetDatabase(baseSettings.Db.AssetBalanceChanges.DbName), log);
        }

        public static AzureBlobStorage CreateMainChainBlobStorage(BaseSettings baseSettings, ILog log)
        {
            return new AzureBlobStorage(baseSettings.Db.AssetsConnString);
        }
    }
}
