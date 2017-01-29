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
using Services.Address;
using Services.Asset;
using Services.BalanceChanges;
using Services.BalanceReport;
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
            ioc.RegisterPerCall<ITransactionService, TransactionService>();
            ioc.RegisterPerCall<IAddressService, AddressService>();
            ioc.RegisterPerCall<ISearchService, SearchService>();
            ioc.RegisterPerCall<IReportRender, PdfReportRenderer>();
            ioc.RegisterPerCall<ITemplateGenerator, RemoteTemplateGenerator>();


            ioc.Register<IEmailSender>(ServiceFactories.CreateEmailSenderProducer(baseSettings, log));

            ioc.RegisterFactorySingleTone(() =>
                new CachedDataDictionary<string, IAssetDefinition>(
                    async () => AssetIndexer.IndexAssetsDefinitions(await ioc.GetObject<IAssetDefinitionRepository>().GetAllAsync())
                    , validDataInSeconds: 1 * 10 * 60));

            ioc.RegisterFactorySingleTone(() =>
                new CachedDataDictionary<string, IAssetCoinholdersIndex>(
                    async () => AssetIndexer.IndexAssetCoinholders(await ioc.GetObject<IAssetCoinholdersIndexRepository>().GetAllAsync())
                    , validDataInSeconds: 1 * 10 * 10));

            ioc.RegisterFactorySingleTone(() =>
                new CachedDataDictionary<string, IAssetScore>(
                    async () => AssetIndexer.IndexAssetScores(await ioc.GetObject<IAssetScoreRepository>().GetAllAsync())
                    , validDataInSeconds: 1 * 10 * 10));

            ioc.RegisterPerCall<IAssetService, AssetService>();
        }
    }

    //public class ServiceBinderOptions
    //{
    //    public string MainChainLocalCacheFileName { get; set; }

    //    public static ServiceBinderOptions Create(string mainChainLocalCacheFileName)
    //    {
    //        return new ServiceBinderOptions
    //        {
    //            MainChainLocalCacheFileName = mainChainLocalCacheFileName
    //        };
    //    }

    //    public static ServiceBinderOptions Default()
    //    {
    //        return new ServiceBinderOptions
    //        {
    //            MainChainLocalCacheFileName = 
    //        }
    //    }
    //}
}
