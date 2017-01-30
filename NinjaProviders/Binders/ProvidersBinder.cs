using Common.IocContainer;
using Common.Log;
using Core.Settings;
using Providers.BlockChainReader;
using Providers.Providers.Lykke.API;
using Providers.Providers.Ninja;

namespace Providers.Binders
{
    public static class ProvidersBinder
    {
        public static void BindProviders(this IoC ioc, BaseSettings baseSettings, ILog log)
        {
            ioc.RegisterPerCall<HttpReader>();

            ioc.RegisterPerCall<NinjaBlockProvider>();
            ioc.RegisterPerCall<NinjaTransactionProvider>();
            ioc.RegisterPerCall<NinjaBlockChainReader>();
            ioc.RegisterPerCall<NinjaSearchProvider>();
            ioc.RegisterPerCall<NinjaAddressProvider>();

            ioc.RegisterPerCall<AssetReader>();

            ioc.RegisterPerCall<LykkeAPIProvider>();

            ioc.RegisterSingleTone<IndexerClientFactory>();
        }
    }
}
