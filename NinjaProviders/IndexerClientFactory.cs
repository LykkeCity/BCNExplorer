using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using Core.Settings;
using NBitcoin.Indexer;
using Providers.Binders;

namespace Providers
{
    public class IndexerClientFactory
    {
        private readonly ILog _log;
        private readonly BaseSettings _baseSettings;

        public IndexerClientFactory(ILog log, BaseSettings baseSettings)
        {
            _log = log;
            _baseSettings = baseSettings;
        }

        public IndexerClient GetIndexerClient()
        {
            return ProvidersFactories.CreateNinjaIndexerClient(_baseSettings, _log);
        }
    }
}
