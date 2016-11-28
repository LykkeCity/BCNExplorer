using Common;
using Common.IocContainer;
using Common.Log;
using Core.Asset;
using Core.Settings;
using Providers.BlockChainReader;
using Providers.Providers.Ninja;

namespace Providers
{
    public static class ProvidersBinder
    {
        public static void BindProviders(this IoC ioc, BaseSettings baseSettings, ILog log)
        {
            ioc.BindCommon();
            ioc.BindNinjaProviders();
            ioc.BindLykkeProviders();
            ioc.BindCommonProviders();
            //ioc.RegisterFactoryPerCall(()=> ProvidersFactories.CreateNinjaIndexerClient(baseSettings, log));
            ioc.RegisterSingleTone<IndexerClientFactory>();
        }

        private static void BindCommon(this IoC ioc)
        {
            ioc.RegisterPerCall<HttpReader>();
        }

        private static void BindLykkeProviders(this IoC ioc)
        {
            ioc.RegisterPerCall<AssetReader>();


        }

        private static void BindNinjaProviders(this IoC ioc)
        {
            ioc.RegisterPerCall<NinjaBlockProvider>();
            ioc.RegisterPerCall<NinjaTransactionProvider>();
            ioc.RegisterPerCall<NinjaBlockChainReader>();
            ioc.RegisterPerCall<NinjaSearchProvider>();
            ioc.RegisterPerCall<NinjaAddressProvider>();
        }

        private static void BindCommonProviders(this IoC ioc)
        {

        }
    }
}
