﻿using AzureRepositories.Asset;
using AzureRepositories.AssetCoinHolders;
using AzureRepositories.AssetDefinition;
using AzureRepositories.BalanceReport;
using AzureRepositories.TransactionCache;
using AzureStorage.Blob;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Log;
using Core.Asset;
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
        public static AssetImageRepository CreateAssetImageRepository(BaseSettings baseSettings, ILog log)
        {
            return new AssetImageRepository(new AzureTableStorage<AssetImageImageEntity>(baseSettings.Db.AssetsConnString, "AssetImages", log));
        }

        public static AssetDataCommandProducer CreateAssetDataCommandProducer(BaseSettings baseSettings, ILog log)
        {
            return new AssetDataCommandProducer(new AzureQueueExt(baseSettings.Db.AssetsConnString,  JobsQueueNames.UpdateAssetDataCommands));
        }

        public static AssetImageCommandProducer CreateAssetImageCommandProducer(BaseSettings baseSettings, ILog log)
        {
            return new AssetImageCommandProducer(new AzureQueueExt(baseSettings.Db.AssetsConnString, JobsQueueNames.UpdateAssetImageCommands));
        }

        public static AssetDefinitionParseBlockCommandProducer CreateAssetDefinitionParseBlockCommandProducer(BaseSettings baseSettings, ILog log)
        {
            return new AssetDefinitionParseBlockCommandProducer(new AzureQueueExt(baseSettings.Db.AssetsConnString, JobsQueueNames.AssetDefinitionParseBlockCommands));
        }

        public static AssetChangesParseBlockCommandProducer CreateAssetChangesParseBlockCommandProducer(BaseSettings baseSettings, ILog log)
        {
            return new AssetChangesParseBlockCommandProducer(new AzureQueueExt(baseSettings.Db.AssetsConnString, JobsQueueNames.AssetChangesParseBlockCommands));
        }

        public static SendBalanceReportCommandQueryProducer CreateSendBalanceReportCommandQueryProducer(BaseSettings baseSettings, ILog log)
        {
            return new SendBalanceReportCommandQueryProducer(new AzureQueueExt(baseSettings.Db.SharedStorageConnString, JobsQueueNames.BalaceReporting), log);
        }
        

        public static AssetCoinholdersIndexesCommandProducer CreateAssetCoinholdersIndexesCommandProducer(BaseSettings baseSettings, ILog log)
        {
            return new AssetCoinholdersIndexesCommandProducer(new AzureQueueExt(baseSettings.Db.AssetsConnString, JobsQueueNames.AssetCoinhodersIndexesCommands), log);
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

        public static TransactionCachedStatusRepository CreateTransactionCacheStatusRepository(BaseSettings baseSettings, ILog log)
        {
            return new TransactionCachedStatusRepository(new AzureTableStorage<TransactionCachedStatusEntity>(baseSettings.Db.AssetsConnString, "TransactionCacheStatuses", log));
        }

        public static TransactionCacheItemRepository CreateTransactionCacheItemRepository(BaseSettings baseSettings, ILog log)
        {
            return new TransactionCacheItemRepository(new AzureBlobStorage(baseSettings.Db.AssetsConnString));
        }

        public static AzureBlobStorage CreateMainChainBlobStorage(BaseSettings baseSettings, ILog log)
        {
            return new AzureBlobStorage(baseSettings.Db.AssetsConnString);
        }
    }
}
