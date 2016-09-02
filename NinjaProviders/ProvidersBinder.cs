using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.IocContainer;
using Providers.BlockChainReader;
using Providers.Contracts.Asset;
using Providers.Providers;
using Providers.Providers.Asset;
using Providers.Providers.Common;
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
            ioc.BindCommonProviders();
        }

        private static void BindCommon(this IoC ioc)
        {
            ioc.RegisterPerCall<HttpReader>();
        }

        private static void BindLykkeProviders(this IoC ioc)
        {
            ioc.RegisterPerCall<AssetReader>();
            
            ioc.RegisterFactorySingleTone(() =>
                new CachedDataDictionary<string, AssetContract>(
                    async () => await ioc.GetObject<AssetReader>().GetDictionaryAsync(Constants.AssetDefinitonUrls)
                    , validDataInSeconds: 60*60*1));

            ioc.RegisterPerCall<AssetProvider>();
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
            ioc.RegisterPerCall<SearchProvider>();
        }
    }
}
