using System.Linq;
using AzureRepositories.Binders;
using Common;
using Common.Cache;
using Common.HttpRemoteRequests;
using Common.IocContainer;
using Common.Log;
using Core.AddressService;
using Core.Asset;
using Core.BalanceReport;
using Core.Block;
using Core.Email;
using Core.SearchService;
using Core.Settings;
using Core.Transaction;
using Providers;
using Providers.Providers.Asset;
using Providers.Providers.Lykke.API;
using Services.Address;
using Services.Asset;
using Services.BalanceChanges;
using Services.BalanceReport;
using Services.Block;
using Services.BlockChain;
using Services.Email;
using Services.MainChain;
using Services.Search;
using Services.Transaction;

namespace Services.Binders
{
    public static class ServicesBinder
    {
        public static void BindServices(this IoC ioc, BaseSettings baseSettings, ILog log)
        {
            ioc.RegisterPerCall<HttpRequestClient>();

            ioc.RegisterFactorySingleTone(()=> new MainChainService(ioc.GetObject<IndexerClientFactory>(), AzureRepoFactories.CreateMainChainBlobStorage(baseSettings, log), log, baseSettings));
            ioc.RegisterSingleTone<BalanceChangesService>();
            ioc.RegisterFactorySingleTone( ()=> new CachedMainChainService(ioc.GetObject<MainChainService>(), new MemoryCacheManager(), cachedTimeInMinutes: 10));

            ioc.RegisterPerCall<IBlockService, BlockService>();
            ioc.RegisterFactorySingleTone(() => (ICachedBlockService) new CachedBlockService(new MemoryCacheManager(), ioc.GetObject<IBlockService>()));
            ioc.RegisterFactorySingleTone(() => (ICachedTransactionService)new CachedTransactionService(new MemoryCacheManager(), ioc.GetObject<ITransactionService>()));



            ioc.RegisterPerCall<ITransactionService, TransactionService>();
            ioc.RegisterPerCall<IAddressService, AddressService>();
            ioc.RegisterPerCall<ISearchService, SearchService>();
            ioc.RegisterPerCall<IReportRenderer, PdfReportRenderer>();
            ioc.RegisterPerCall<ITemplateGenerator, RemoteTemplateGenerator>();
            ioc.RegisterSingleTone<FiatRatesService>();
            

            ioc.Register<IEmailSender>(ServiceFactories.CreateEmailSenderProducer(baseSettings, log));
            ioc.Register<IAssetImageCacher>(ServiceFactories.CreateIAssetImageCacher(baseSettings, log));

            ioc.RegisterFactorySingleTone(() =>
                new CachedDataDictionary<string, IAssetDefinition>(
                    async () => AssetIndexer.IndexAssetsDefinitions(await ioc.GetObject<IAssetDefinitionRepository>().GetAllAsync(), await ioc.GetObject<IAssetImageRepository>().GetAllAsync())
                    , validDataInSeconds: 1 * 10 * 60));

            ioc.RegisterFactorySingleTone(() =>
                new CachedDataDictionary<string, IAssetCoinholdersIndex>(
                    async () => AssetIndexer.IndexAssetCoinholders(await ioc.GetObject<IAssetCoinholdersIndexRepository>().GetAllAsync())
                    , validDataInSeconds: 1 * 10 * 10));

            ioc.RegisterFactorySingleTone(() =>
                new CachedDataDictionary<string, IAssetScore>(
                    async () => AssetIndexer.IndexAssetScores(await ioc.GetObject<IAssetScoreRepository>().GetAllAsync())
                    , validDataInSeconds: 1 * 10 * 10));

            ioc.RegisterFactorySingleTone(() =>
                new CachedDataDictionary<string, IAsset>(
                    async () => (await ioc.GetObject<LykkeAPIProvider>().GetAssetsAsync()).ToDictionary(p => p.Id)
                    , validDataInSeconds: 2 * 60 * 60));


            ioc.RegisterFactorySingleTone(() =>
                new CachedDataDictionary<string, IAssetPair>(
                    async () => (await ioc.GetObject<LykkeAPIProvider>().GetAssetPairDictionary()).ToDictionary(p => p.Id)
                    , validDataInSeconds: 2 * 60 * 60));

            ioc.RegisterPerCall<IAssetService, AssetService>();
        }
    }
}
