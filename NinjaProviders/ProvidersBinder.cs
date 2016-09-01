using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.IocContainer;
using Providers.BlockChainReader;
using Providers.Contracts.Lykke;
using Providers.Providers;
using Providers.Providers.Lykke;
using Providers.Providers.Ninja;

namespace Providers
{
    public static class ProvidersBinder
    {
        public static void BindProviders(this IoC ioc)
        {
            ioc.BindCommon();
            ioc.BindNinjaProviders();
            ioc.BindLykkeProviders();
        }

        private static void BindCommon(this IoC ioc)
        {
            ioc.RegisterPerCall<HttpReader>();
        }

        private static void BindLykkeProviders(this IoC ioc)
        {
            ioc.RegisterPerCall<LykkeBlockChainReader>();
            
            ioc.RegisterFactorySingleTone(() =>
                new CachedDataDictionary<string, LykkeAssetContract>(
                    async () => await ioc.GetObject<LykkeBlockChainReader>().GetDictionaryAsync(LykkeConstants.AssetUrls)
                    ));

            ioc.RegisterPerCall<LykkeAssetProvider>();
        }

        private static void BindNinjaProviders(this IoC ioc)
        {
            ioc.RegisterPerCall<NinjaBlockProvider>();
            ioc.RegisterPerCall<NinjaTransactionProvider>();
            ioc.RegisterPerCall<NinjaBlockChainReader>();
            ioc.RegisterPerCall<NinjaSearchProvider>();
            ioc.RegisterPerCall<NinjaAddressProvider>();
        }
    }
}
