﻿using Common;
using Common.IocContainer;
using Common.Log;
using Core.AddressService;
using Core.Asset;
using Core.Block;
using Core.SearchService;
using Core.Settings;
using Core.Transaction;
using Providers.Providers.Asset;
using Services.Address;
using Services.Asset;
using Services.BalanceChanges;
using Services.BlockChain;
using Services.MainChain;
using Services.Search;
using Services.Transaction;

namespace Services.Binders
{
    public static class ServicesBinder
    {
        public static void BindServices(this IoC ioc, BaseSettings baseSettings, ILog log)
        {
            ioc.RegisterSingleTone<MainChainRepository>();
            ioc.RegisterSingleTone<BalanceChangesService>();

            ioc.RegisterPerCall<IBlockService, BlockService>();
            ioc.RegisterPerCall<ITransactionService, TransactionService>();
            ioc.RegisterPerCall<IAddressService, AddressService>();
            ioc.RegisterPerCall<ISearchService, SearchService>();

            ioc.RegisterFactorySingleTone(() =>
                new CachedDataDictionary<string, IAssetDefinition>(
                    async () => AssetIndexer.IndexAssetsDefinitions(await ioc.GetObject<IAssetDefinitionRepository>().GetAllAsync())
                    , validDataInSeconds: 1 * 10 * 60));

            ioc.RegisterFactorySingleTone(() =>
                new CachedDataDictionary<string, IAssetCoinholdersIndex>(
                    async () => AssetIndexer.IndexAssetCoinholders(await ioc.GetObject<IAssetCoinholdersIndexRepository>().GetAllAsync())
                    , validDataInSeconds: 1 * 10 * 60));

            ioc.RegisterFactorySingleTone(() =>
                new CachedDataDictionary<string, IAssetScore>(
                    async () => AssetIndexer.IndexAssetScores(await ioc.GetObject<IAssetScoreRepository>().GetAllAsync())
                    , validDataInSeconds: 1 * 10 * 60));

            ioc.RegisterPerCall<IAssetService, AssetService>();
        }
    }
}
